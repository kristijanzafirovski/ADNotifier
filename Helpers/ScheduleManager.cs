using System.Text.Json;
using GimNotifier.Models;

namespace GimNotifier.Helpers
{
    public static class ScheduleManager
    {
        private static readonly string _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GimNotifier", "schedule.json");

        public static List<ScheduledMessage> Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path);
                    return JsonSerializer.Deserialize<List<ScheduledMessage>>(json) ?? new();
                }
            }
            catch { }
            return new();
        }

        public static void Save(List<ScheduledMessage> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
