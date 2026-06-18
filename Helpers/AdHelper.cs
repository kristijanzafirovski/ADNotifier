using System.DirectoryServices;
using GimNotifier.Models;

namespace GimNotifier.Helpers
{
    public static class AdHelper
    {
        public static string Domain => SettingsManager.Load().DomainDN;

        public static void LoadChildOUs(TreeNode parentNode, string parentDN)
        {
            using var entry = new DirectoryEntry($"LDAP://{parentDN}");
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = "(objectClass=organizationalUnit)",
                SearchScope = SearchScope.OneLevel
            };

            searcher.PropertiesToLoad.Add("name");
            searcher.PropertiesToLoad.Add("distinguishedName");
            searcher.Sort = new SortOption("name", SortDirection.Ascending);

            foreach (SearchResult result in searcher.FindAll())
            {
                string name = result.Properties["name"][0].ToString()!;
                string dn   = result.Properties["distinguishedName"][0].ToString()!;

                var node = new TreeNode(name) { Tag = dn };
                parentNode.Nodes.Add(node);

                if (HasChildOUs(dn))
                    node.Nodes.Add(new TreeNode("Loading...") { Tag = "__dummy__" });
            }
        }

        public static bool HasChildOUs(string dn)
        {
            try
            {
                using var entry = new DirectoryEntry($"LDAP://{dn}");
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = "(objectClass=organizationalUnit)",
                    SearchScope = SearchScope.OneLevel
                };
                searcher.PropertiesToLoad.Add("name");
                return searcher.FindOne() != null;
            }
            catch { return false; }
        }

        public static List<ComputerEntry> GetComputers(string ouDN, string ouName)
        {
            var list = new List<ComputerEntry>();
            try
            {
                using var entry = new DirectoryEntry($"LDAP://{ouDN}");
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = "(&(objectClass=computer)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                    SearchScope = SearchScope.OneLevel
                };

                searcher.PropertiesToLoad.Add("cn");
                searcher.Sort = new SortOption("cn", SortDirection.Ascending);

                foreach (SearchResult result in searcher.FindAll())
                {
                    list.Add(new ComputerEntry
                    {
                        Name   = result.Properties["cn"][0].ToString()!,
                        OUName = ouName,
                        OUDN   = ouDN
                    });
                }
            }
            catch { }
            return list;
        }

        public static List<ComputerEntry> SearchComputers(string query)
        {
            var list = new List<ComputerEntry>();
            try
            {
                using var entry = new DirectoryEntry($"LDAP://{Domain}");
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = $"(&(objectClass=computer)(cn=*{query}*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                    SearchScope = SearchScope.Subtree
                };

                searcher.PropertiesToLoad.Add("cn");
                searcher.PropertiesToLoad.Add("distinguishedName");
                searcher.Sort = new SortOption("cn", SortDirection.Ascending);

                foreach (SearchResult result in searcher.FindAll())
                {
                    string dn   = result.Properties["distinguishedName"][0].ToString()!;
                    string ouDN = dn[(dn.IndexOf(',') + 1)..];
                    string ouName = ouDN.Split(',')[0].Replace("OU=", "").Replace("DC=", "");

                    list.Add(new ComputerEntry
                    {
                        Name   = result.Properties["cn"][0].ToString()!,
                        OUName = ouName,
                        OUDN   = ouDN
                    });
                }
            }
            catch { }
            return list;
        }
    }
}
