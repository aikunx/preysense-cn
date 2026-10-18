using System.IO;

namespace PreySense
{
    public static class AppLogger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
        private static readonly object LockObj = new();

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            // Only log errors, warnings, failures, and critical issues for performance & lightweight disk usage
            bool isErrorOrWarning = message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                                    message.Contains("fail", StringComparison.OrdinalIgnoreCase) ||
                                    message.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
                                    message.Contains("warning", StringComparison.OrdinalIgnoreCase) ||
                                    message.Contains("conflict", StringComparison.OrdinalIgnoreCase);

            if (!isErrorOrWarning) return;

            lock (LockObj)
            {
                try
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogPath, logLine);
                    System.Diagnostics.Debug.Write(logLine);
                }
                catch
                {
                    // Ignore logging errors to prevent crashes
                }
            }
        }

        public static void Clear()
        {
            lock (LockObj)
            {
                try
                {
                    if (File.Exists(LogPath))
                    {
                        File.Delete(LogPath);
                    }
                }
                catch { }
            }
        }
    }
}
