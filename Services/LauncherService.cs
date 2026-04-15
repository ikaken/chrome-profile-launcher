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
        private readonly string _userDataPath;
        private readonly IFileSystem _fileSystem;
        private static readonly Regex ProfileRegex = new Regex(@"--profile-directory[= ]""?([^""\s]+)""?", RegexOptions.Compiled);

        public LauncherService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _chromePath = GetChromePath();
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _userDataPath = Path.Combine(localAppData, "Google", "Chrome", "User Data");
        }

        public async void LaunchOrFocus(ProfileInfo profile)
        {
            Logger.Info($"[LauncherService] Request: {profile.DisplayName} ({profile.Id})");

            // 1. Check cached HWND
            if (profile.Hwnd != IntPtr.Zero)
            {
                if (IsWindowValid(profile.Hwnd))
                {
                    FocusWindow(profile.Hwnd);
                    return;
                }
                profile.Hwnd = IntPtr.Zero;
            }

            // 2. Check SingletonLock (Early exit if not running)
            if (!IsProfileRunningByLock(profile.Id))
            {
                Logger.Info($"[LauncherService] Profile not running (No LOCK). Launching...");
                await LaunchAndWaitForWindow(profile);
                return;
            }

            // 3. Scan existing windows
            var hwnd = FindWindowForProfile(profile);
            if (hwnd != IntPtr.Zero)
            {
                Logger.Info($"[LauncherService] Identified existing window: {hwnd}");
                profile.Hwnd = hwnd;
                FocusWindow(hwnd);
                return;
            }

            Logger.Info($"[LauncherService] LOCK exists but no matching window found for {profile.Id}. Fallback to Launch.");
            await LaunchAndWaitForWindow(profile);
        }

        private bool IsProfileRunningByLock(string profileId)
        {
            var lockPath = Path.Combine(_userDataPath, profileId, "SingletonLock");
            return _fileSystem.FileExists(lockPath);
        }

        private bool IsWindowValid(IntPtr hwnd)
        {
            if (!Win32Api.IsWindow(hwnd) || !Win32Api.IsWindowVisible(hwnd))
                return false;

            // 1. Class name check
            var sbClass = new StringBuilder(256);
            Win32Api.GetClassName(hwnd, sbClass, sbClass.Capacity);
            if (sbClass.ToString() != "Chrome_WidgetWin_1")
                return false;

            // 2. Process name check (Distinguishes Chrome from Edge, etc.)
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
            var beforePids = Process.GetProcessesByName("chrome").Select(p => (uint)p.Id).ToHashSet();
            
            Launch(profile);

            for (int i = 0; i < 15; i++) // 7.5s limit
            {
                await Task.Delay(500);
                
                // 1. Check window difference (Primary)
                var currentHwnds = GetCurrentChromeWindows();
                var newHwnds = currentHwnds.Except(beforeHwnds).ToList();

                if (newHwnds.Count > 0)
                {
                    var bestHwnd = newHwnds.First();
                    Logger.Info($"[LauncherService] New window detected via HWND diff: {bestHwnd} -> {profile.Id}");
                    profile.Hwnd = bestHwnd;
                    FocusWindow(bestHwnd);
                    return;
                }

                // 2. Check process difference (Auxiliary)
                var currentProcs = Process.GetProcessesByName("chrome");
                var newPids = currentProcs.Select(p => (uint)p.Id).Except(beforePids).ToList();
                
                if (newPids.Count > 0)
                {
                    var newProcArray = currentProcs.Where(p => newPids.Contains((uint)p.Id)).ToArray();
                    var cmdMap = GetChromeProcessCommandLineMap(newProcArray);
                    foreach (var pid in newPids)
                    {
                        if (cmdMap.TryGetValue(pid, out string? cmd) && cmd != null && IsProfileMatch(cmd, profile.Id))
                        {
                            var hwnd = FindWindowForPid(pid);
                            if (hwnd != IntPtr.Zero)
                            {
                                Logger.Info($"[LauncherService] New window detected via PID diff: {hwnd} (PID:{pid}) -> {profile.Id}");
                                profile.Hwnd = hwnd;
                                FocusWindow(hwnd);
                                return;
                            }
                        }
                    }
                }
            }
            Logger.Info($"[LauncherService] Timeout: No new Chrome window appeared for {profile.Id}");
        }

        private IntPtr FindWindowForPid(uint targetPid)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            Win32Api.EnumWindows((hwnd, _) =>
            {
                if (IsWindowValid(hwnd))
                {
                    Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                    if (pid == targetPid)
                    {
                        foundHwnd = hwnd;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);
            return foundHwnd;
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
                if (!IsWindowValid(hwnd)) return true;

                Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                string cmd = cmdMap.ContainsKey(pid) ? cmdMap[pid] : "";

                int score = CalculateMatchScore(hwnd, pid, cmd, profile);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestHwnd = hwnd;
                }
                return score < 80; // Stop if perfect match
            }, IntPtr.Zero);

            return maxScore >= 30 ? bestHwnd : IntPtr.Zero;
        }

        private Dictionary<uint, string> GetChromeProcessCommandLineMap(Process[] processes)
        {
            var map = new Dictionary<uint, string>();
            if (processes.Length == 0) return map;

            try
            {
                var filter = string.Join(" OR ", processes.Select(p => $"ProcessId={p.Id}"));
                using var searcher = new ManagementObjectSearcher($"SELECT ProcessId, CommandLine FROM Win32_Process WHERE {filter}");
                foreach (var obj in searcher.Get())
                {
                    var pid = (uint)obj["ProcessId"];
                    var cmd = obj["CommandLine"]?.ToString() ?? "";
                    map[pid] = cmd;
                }
            } catch { }
            return map;
        }

        private int CalculateMatchScore(IntPtr hwnd, uint pid, string commandLine, ProfileInfo profile)
        {
            int score = 0;

            // 1. Title Match (50 points)
            var sb = new StringBuilder(512);
            Win32Api.GetWindowText(hwnd, sb, sb.Capacity);
            string title = sb.ToString();
            
            if (!string.IsNullOrEmpty(profile.DisplayName) && title.IndexOf(profile.DisplayName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score += 50;
            }
            
            // 2. Command Line Match (30 points)
            if (IsProfileMatch(commandLine, profile.Id))
            {
                score += 30;
            }

            return score > 0 ? score : -1;
        }

        private bool IsProfileMatch(string commandLine, string targetProfileId)
        {
            if (string.IsNullOrEmpty(commandLine)) return targetProfileId == "Default";

            var match = ProfileRegex.Match(commandLine);
            if (match.Success)
            {
                return string.Equals(match.Groups[1].Value, targetProfileId, StringComparison.OrdinalIgnoreCase);
            }

            if (targetProfileId == "Default")
            {
                // If no profile-directory is specified, it's the Default profile.
                return !commandLine.Contains("--profile-directory");
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
