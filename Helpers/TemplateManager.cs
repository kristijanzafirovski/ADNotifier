using System.Text.Json;
using GimNotifier.Models;

namespace GimNotifier.Helpers
{
    public static class TemplateManager
    {
        private static readonly string _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GimNotifier", "templates.json");

        public static List<MessageTemplate> Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path);
                    return JsonSerializer.Deserialize<List<MessageTemplate>>(json) ?? new();
                }
            }
            catch { }
            return new();
        }

        public static void Save(List<MessageTemplate> templates)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
