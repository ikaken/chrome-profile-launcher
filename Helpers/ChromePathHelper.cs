using System;
using System.IO;
using ChromeProfileLauncher.Services;

namespace ChromeProfileLauncher.Helpers
{
    public static class ChromePathHelper
    {
        public static string GetChromePath(IFileSystem fileSystem)
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
            };
            foreach (var path in paths)
            {
                if (fileSystem.FileExists(path)) return path;
            }
            return string.Empty;
        }
    }
}
