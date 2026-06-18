using System.Diagnostics;
using GimNotifier.Models;

namespace GimNotifier.Helpers
{
    public static class MsgSender
    {
        public static (bool ok, string error) Send(string computer, string message)
        {
            try
            {
                var settings = SettingsManager.Load();
                var psi = new ProcessStartInfo
                {
                    FileName = "msg.exe",
                    Arguments = $"/SERVER:{computer} * \"{message}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute  = false,
                    CreateNoWindow   = true
                };

                using var proc = Process.Start(psi)!;
                proc.WaitForExit(settings.MsgTimeoutMs);

                if (proc.ExitCode == 0)
                    return (true, "");

                string err = proc.StandardError.ReadToEnd().Trim();
                return (false, string.IsNullOrEmpty(err) ? $"Exit code {proc.ExitCode}" : err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static void AppendLog(string line)
        {
            try
            {
                var settings = SettingsManager.Load();
                if (!settings.SaveLogs) return;

                Directory.CreateDirectory(settings.LogPath);
                string file = Path.Combine(settings.LogPath, $"{DateTime.Now:dd-MM-yyyy}.log");
                File.AppendAllText(file, $"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
