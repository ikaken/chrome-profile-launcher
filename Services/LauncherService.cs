using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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
        private static readonly Regex ProfileRegex = new Regex(@"--profile-directory[= ]""?([^""\s]+)""?", RegexOptions.Compiled);

        public LauncherService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _chromePath = GetChromePath();
        }

        public async void LaunchOrFocus(ProfileInfo profile)
        {
            Logger.Info($"[LauncherService] Request: {profile.DisplayName} ({profile.Id})");

            if (profile.Hwnd != IntPtr.Zero)
            {
                if (IsWindowValid(profile.Hwnd))
                {
                    FocusWindow(profile.Hwnd);
                    return;
                }
                profile.Hwnd = IntPtr.Zero;
            }

            var hwnd = FindWindowForProfile(profile);
            if (hwnd != IntPtr.Zero)
            {
                Logger.Info($"[LauncherService] Identified existing window: {hwnd}");
                profile.Hwnd = hwnd;
                FocusWindow(hwnd);
                return;
            }

            Logger.Info($"[LauncherService] No valid window found. Launching with differential detection...");
            await LaunchAndWaitForWindow(profile);
        }

        private bool IsWindowValid(IntPtr hwnd)
        {
            if (!Win32Api.IsWindow(hwnd) || !Win32Api.IsWindowVisible(hwnd))
                return false;

            // Class name check
            var sbClass = new StringBuilder(256);
            Win32Api.GetClassName(hwnd, sbClass, sbClass.Capacity);
            if (sbClass.ToString() != "Chrome_WidgetWin_1")
                return false;

            // CRITICAL: Process name check to exclude Slack, VS Code, Discord etc.
            try
            {
                Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                using (var proc = Process.GetProcessById((int)pid))
                {
                    if (!proc.ProcessName.Equals("chrome", StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }
            catch { return false; }

            return true;
        }

        private async Task LaunchAndWaitForWindow(ProfileInfo profile)
        {
            var beforeHwnds = GetCurrentChromeWindows();
            Launch(profile);

            for (int i = 0; i < 15; i++) // Increased to 7.5s for stability
            {
                await Task.Delay(500);
                var currentHwnds = GetCurrentChromeWindows();
                var newHwnds = currentHwnds.Except(beforeHwnds).ToList();

                if (newHwnds.Count > 0)
                {
                    var bestHwnd = newHwnds.First();
                    Logger.Info($"[LauncherService] New Chrome window detected: {bestHwnd} -> {profile.Id}");
                    profile.Hwnd = bestHwnd;
                    FocusWindow(bestHwnd);
                    return;
                }
            }
            Logger.Info($"[LauncherService] Timeout: No new Chrome window appeared for {profile.Id}");
        }

        private HashSet<IntPtr> GetCurrentChromeWindows()
        {
            var hwnds = new HashSet<IntPtr>();
            Win32Api.EnumWindows((hwnd, _) =>
            {
                if (IsWindowValid(hwnd)) hwnds.Add(hwnd);
                return true;
            }, IntPtr.Zero);
            return hwnds;
        }

        private void FocusWindow(IntPtr hwnd)
        {
            Win32Api.ShowWindow(hwnd, Win32Api.SW_RESTORE);

            IntPtr fg = Win32Api.GetForegroundWindow();
            uint fgThread = Win32Api.GetWindowThreadProcessId(fg, out _);
            uint targetThread = Win32Api.GetWindowThreadProcessId(hwnd, out _);

            if (fgThread != targetThread)
            {
                Win32Api.AttachThreadInput(fgThread, targetThread, true);
                Win32Api.SetForegroundWindow(hwnd);
                Win32Api.AttachThreadInput(fgThread, targetThread, false);
            }
            else
            {
                Win32Api.SetForegroundWindow(hwnd);
            }
        }

        private void Launch(ProfileInfo profile)
        {
            if (string.IsNullOrEmpty(_chromePath) || !_fileSystem.FileExists(_chromePath))
                throw new FileNotFoundException("Chrome not found.");

            Process.Start(new ProcessStartInfo
            {
                FileName = _chromePath,
                Arguments = $"--profile-directory=\"{profile.Id}\"",
                UseShellExecute = true
            });
        }

        private IntPtr FindWindowForProfile(ProfileInfo profile)
        {
            var procs = Process.GetProcessesByName("chrome");
            if (procs.Length == 0) return IntPtr.Zero;

            var cmdMap = GetChromeProcessCommandLineMap(procs);
            IntPtr bestHwnd = IntPtr.Zero;
            int maxScore = -1;

            Win32Api.EnumWindows((hwnd, _) =>
            {
                // Note: IsWindowValid now checks for ProcessName == "chrome"
                if (!IsWindowValid(hwnd)) return true;

                Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                string cmd = cmdMap.ContainsKey(pid) ? cmdMap[pid] : "";

                int score = CalculateMatchScore(hwnd, pid, cmd, profile);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestHwnd = hwnd;
                }
                return score < 100;
            }, IntPtr.Zero);

            return maxScore >= 50 ? bestHwnd : IntPtr.Zero;
        }

        private Dictionary<uint, string> GetChromeProcessCommandLineMap(Process[] processes)
        {
            var map = new Dictionary<uint, string>();
            try
            {
                var filter = string.Join(" OR ", processes.Select(p => $"ProcessId={p.Id}"));
                using var searcher = new ManagementObjectSearcher($"SELECT ProcessId, CommandLine FROM Win32_Process WHERE {filter}");
                foreach (var obj in searcher.Get()) map[(uint)obj["ProcessId"]] = obj["CommandLine"]?.ToString() ?? "";
            } catch { }
            return map;
        }

        private int CalculateMatchScore(IntPtr hwnd, uint pid, string commandLine, ProfileInfo profile)
        {
            // 1. AUMID (100)
            string aumid = GetAppUserModelId(hwnd);
            if (!string.IsNullOrEmpty(aumid))
            {
                if (aumid.IndexOf(profile.Id, StringComparison.OrdinalIgnoreCase) >= 0) return 100;
                if (profile.Id == "Default" && aumid.Equals("Chrome", StringComparison.OrdinalIgnoreCase)) return 100;
            }

            int score = 0;

            // 2. Title (50)
            var sb = new StringBuilder(512);
            Win32Api.GetWindowText(hwnd, sb, sb.Capacity);
            string title = sb.ToString();
            if (!string.IsNullOrEmpty(profile.DisplayName) && title.IndexOf(profile.DisplayName, StringComparison.OrdinalIgnoreCase) >= 0) score += 50;
            else if (profile.Id == "Default" && title.IndexOf("Google Chrome", StringComparison.OrdinalIgnoreCase) >= 0) score += 50;

            // 3. Command Line (30)
            if (IsProfileMatch(commandLine, profile.Id)) score += 30;

            return score > 0 ? score : -1;
        }

        private string GetAppUserModelId(IntPtr hwnd)
        {
            try
            {
                Guid guid = Win32Api.IID_IPropertyStore;
                if (Win32Api.SHGetPropertyStoreForWindow(hwnd, ref guid, out object ppv) == 0)
                {
                    var store = (Win32Api.IPropertyStore)ppv;
                    var key = Win32Api.PropertyKey.PKEY_AppUserModel_ID;
                    if (store.GetValue(ref key, out var pv) == 0)
                    {
                        using (pv) return pv.GetValue();
                    }
                }
            } catch { }
            return string.Empty;
        }

        private bool IsProfileMatch(string commandLine, string targetProfileId)
        {
            var match = ProfileRegex.Match(commandLine);
            if (match.Success) return string.Equals(match.Groups[1].Value, targetProfileId, StringComparison.OrdinalIgnoreCase);
            if (targetProfileId == "Default")
            {
                return !commandLine.Contains("--profile-directory") && !commandLine.Contains("--user-data-dir") && !commandLine.Contains("--single-argument");
            }
            return false;
        }

        private string GetChromePath()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
            };
            foreach (var path in paths) if (_fileSystem.FileExists(path)) return path;
            return string.Empty;
        }
    }
}
