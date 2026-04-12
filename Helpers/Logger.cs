using System;
using System.IO;

namespace ChromeProfileLauncher.Helpers
{
    public static class Logger
    {
        private static readonly string LogPath;

        static Logger()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "ChromeProfileLauncher");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            LogPath = Path.Combine(dir, "debug.log");
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message, Exception? ex = null) 
            => Write("ERROR", $"{message}{(ex != null ? $"\n{ex}" : "")}");

        private static void Write(string level, string message)
        {
            try
            {
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                
                // ファイルに書き込み
                File.AppendAllText(LogPath, logLine + Environment.NewLine);
                
                // コンソール/デバッグ出力 (重要)
                Console.WriteLine(logLine);
                System.Diagnostics.Debug.WriteLine(logLine);
            }
            catch
            {
            }
        }
    }
}
