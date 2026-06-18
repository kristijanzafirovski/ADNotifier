namespace GimNotifier.Models
{
    public class AppSettings
    {
        public string Domain { get; set; } = "gim.local";
        public string DomainDN { get; set; } = "DC=gim,DC=local";
        public string LogPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "GIMnotifikacii", "Logs");
        public int MsgTimeoutMs { get; set; } = 5000;
        public bool SaveLogs { get; set; } = true;
    }
}
