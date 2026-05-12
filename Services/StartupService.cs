using Microsoft.Win32;
using System;
using System.Diagnostics;
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
                var value = key?.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value);
            }
        }

        public void Register()
        {
            var exePath = Environment.ProcessPath
                ?? Process.GetCurrentProcess().MainModule?.FileName
                ?? Assembly.GetExecutingAssembly().Location;
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                key?.SetValue(AppName, exePath ?? string.Empty);
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
