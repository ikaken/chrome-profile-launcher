using Microsoft.Win32;
using System.Reflection;

namespace ChromeProfileLauncher.Services
{
    public interface IStartupService
    {
        bool IsRegistered();
        void Register();
        void Unregister();
    }

    public class StartupService : IStartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ChromeProfileLauncher";

        public bool IsRegistered()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                return key?.GetValue(AppName) != null;
            }
        }

        public void Register()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                key?.SetValue(AppName, Assembly.GetExecutingAssembly().Location);
            }
        }

        public void Unregister()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                key?.DeleteValue(AppName, false);
            }
        }
    }
}
