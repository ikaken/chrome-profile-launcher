using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using ChromeProfileLauncher.Helpers;
using ChromeProfileLauncher.Models;

namespace ChromeProfileLauncher.Services
{
    public interface ILauncherService
    {
        void LaunchOrFocus(ProfileInfo profile);
    }

    public class LauncherService : ILauncherService
    {
        private string _chromePath = string.Empty;
        private readonly IFileSystem _fileSystem;

        public LauncherService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _chromePath = GetChromePath();
        }

        public void LaunchOrFocus(ProfileInfo profile)
        {
            var hwnd = FindWindowForProfile(profile.Id);
            if (hwnd != IntPtr.Zero)
            {
                Win32Api.ShowWindow(hwnd, Win32Api.SW_RESTORE);
                Win32Api.SetForegroundWindow(hwnd);
                return;
            }

            Launch(profile);
        }

        private void Launch(ProfileInfo profile)
        {
            if (string.IsNullOrEmpty(_chromePath) || !_fileSystem.FileExists(_chromePath))
                throw new FileNotFoundException("Chrome executable not found.");

            var psi = new ProcessStartInfo
            {
                FileName = _chromePath,
                Arguments = $"--profile-directory=\"{profile.Id}\"",
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private IntPtr FindWindowForProfile(string profileId)
        {
            // Search processes with WMI to find command line
            var query = "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'chrome.exe'";
            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();

            var targetPids = new HashSet<uint>();
            foreach (var obj in results)
            {
                var commandLine = obj["CommandLine"]?.ToString() ?? "";
                if (commandLine.Contains($"--profile-directory=\"{profileId}\"") || 
                    (profileId == "Default" && !commandLine.Contains("--profile-directory")))
                {
                    targetPids.Add((uint)obj["ProcessId"]);
                }
            }

            if (targetPids.Count == 0) return IntPtr.Zero;

            // Find window handle for these PIDs
            IntPtr foundHwnd = IntPtr.Zero;
            Win32Api.EnumWindows((hwnd, lParam) =>
            {
                Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                if (targetPids.Contains(pid))
                {
                    // Check if it's a visible window (basic filter)
                    var sb = new StringBuilder(256);
                    Win32Api.GetWindowText(hwnd, sb, sb.Capacity);
                    var title = sb.ToString();
                    
                    // Chrome main window usually has a title and is a top-level window
                    if (!string.IsNullOrEmpty(title) && title.Contains("Google Chrome"))
                    {
                        foundHwnd = hwnd;
                        return false; // Stop enumeration
                    }
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;
        }

        private string GetChromePath()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
            };

            foreach (var path in paths)
            {
                if (_fileSystem.FileExists(path))
                    return path;
            }

            return string.Empty;
        }
    }
}
