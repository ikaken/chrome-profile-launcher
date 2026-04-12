using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
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
        private static readonly Regex ProfileRegex = new Regex(@"--profile-directory[= ]""?(.+?)""?(?:\s--|$)", RegexOptions.Compiled);

        public LauncherService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _chromePath = GetChromePath();
        }

        public void LaunchOrFocus(ProfileInfo profile)
        {
            Logger.Info($"=== LaunchOrFocus Interaction: Profile={profile.Id} ===");

            // 方式C: ハイブリッド方式 - キャッシュの検証
            if (profile.Hwnd != IntPtr.Zero)
            {
                string hexHwnd = $"0x{profile.Hwnd.ToInt64():X}";
                Logger.Info($"[Cache Check] Found cached HWND: {hexHwnd} for {profile.Id}");
                
                if (IsWindowValid(profile.Hwnd))
                {
                    Logger.Info($"[Cache Hit] HWND {hexHwnd} is still valid. Focusing window.");
                    FocusWindow(profile.Hwnd);
                    return;
                }
                
                Logger.Info($"[Cache Miss] HWND {hexHwnd} is no longer valid or visible. Clearing cache.");
                profile.Hwnd = IntPtr.Zero; // キャッシュが無効な場合はクリア
            }
            else
            {
                Logger.Info($"[No Cache] No HWND cached for {profile.Id}. Starting search.");
            }

            // 方式B: 都度検索（フォールバック）
            var hwnd = FindWindowForProfile(profile.Id);
            if (hwnd != IntPtr.Zero)
            {
                string hexHwnd = $"0x{hwnd.ToInt64():X}";
                Logger.Info($"[Search Match] Valid window found: {hexHwnd}. Saving to cache.");
                profile.Hwnd = hwnd; // 有効なハンドルが見つかったらキャッシュ
                FocusWindow(hwnd);
                return;
            }

            // 見つからなければ新しく起動
            Logger.Info($"[Not Found] No windows for {profile.Id} found on system. Launching new instance.");
            Launch(profile);
        }

        private bool IsWindowValid(IntPtr hwnd)
        {
            if (!Win32Api.IsWindow(hwnd))
            {
                Logger.Info($"HWND {hwnd} is not a valid window.");
                return false;
            }
            if (!Win32Api.IsWindowVisible(hwnd))
            {
                Logger.Info($"HWND {hwnd} is not visible.");
                return false;
            }

            var sb = new StringBuilder(256);
            Win32Api.GetClassName(hwnd, sb, sb.Capacity);
            var className = sb.ToString();
            bool isValidClass = className == "Chrome_WidgetWin_1";
            
            if (!isValidClass)
            {
                Logger.Info($"HWND {hwnd} has invalid class name: '{className}'");
            }
            
            return isValidClass;
        }

        private void FocusWindow(IntPtr hwnd)
        {
            Win32Api.ShowWindow(hwnd, Win32Api.SW_RESTORE);

            IntPtr foregroundWindow = Win32Api.GetForegroundWindow();
            uint foregroundThreadId = Win32Api.GetWindowThreadProcessId(foregroundWindow, out _);
            uint targetThreadId = Win32Api.GetWindowThreadProcessId(hwnd, out _);
            uint currentThreadId = Win32Api.GetCurrentThreadId();

            // AttachThreadInputを使用してフォーカス処理を安定化
            if (foregroundThreadId != targetThreadId)
            {
                Win32Api.AttachThreadInput(foregroundThreadId, targetThreadId, true);
                Win32Api.SetForegroundWindow(hwnd);
                Win32Api.AttachThreadInput(foregroundThreadId, targetThreadId, false);
            }
            else
            {
                Win32Api.SetForegroundWindow(hwnd);
            }
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
            // 改善アプローチ: PID絞り込みによるWMI高速化
            var chromeProcesses = Process.GetProcessesByName("chrome");
            if (chromeProcesses.Length == 0) return IntPtr.Zero;

            var pidFilter = string.Join(" OR ", chromeProcesses.Select(p => $"ProcessId = {p.Id}"));
            var query = "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'chrome.exe'";
            Logger.Info("Dumping ALL Chrome processes for analysis:");

            var targetPids = new HashSet<uint>();
            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                foreach (var obj in results)
                {
                    var pid = (uint)obj["ProcessId"];
                    var commandLine = obj["CommandLine"]?.ToString() ?? "";
                    Logger.Info($"PID: {pid}, CommandLine: {commandLine}");

                    if (IsProfileMatch(commandLine, profileId))
                    {
                        Logger.Info($">>> MATCH FOUND for {profileId} in PID: {pid}");
                        targetPids.Add(pid);
                    }
                }
            }

            Logger.Info("--- Window Enumeration for ALL Chrome PIDs ---");
            var allChromePids = new HashSet<uint>(chromeProcesses.Select(p => (uint)p.Id));
            IntPtr foundHwnd = IntPtr.Zero;
            
            Win32Api.EnumWindows((hwnd, lParam) =>
            {
                Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
                if (allChromePids.Contains(pid))
                {
                    var sb = new StringBuilder(256);
                    Win32Api.GetWindowText(hwnd, sb, sb.Capacity);
                    var title = sb.ToString();
                    
                    if (Win32Api.IsWindowVisible(hwnd))
                    {
                        Logger.Info($"Visible Window: HWND={hwnd}, PID={pid}, Title='{title}'");
                        if (targetPids.Contains(pid) && foundHwnd == IntPtr.Zero)
                        {
                            if (IsWindowValid(hwnd))
                            {
                                Logger.Info($"Selected this window for {profileId}");
                                foundHwnd = hwnd;
                            }
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;
        }

        private bool IsProfileMatch(string commandLine, string targetProfileId)
        {
            var match = ProfileRegex.Match(commandLine);
            if (match.Success)
            {
                var foundId = match.Groups[1].Value;
                bool isMatch = foundId == targetProfileId;
                if (!isMatch)
                {
                    Logger.Info($"Profile mismatch: Found='{foundId}', Target='{targetProfileId}' in CommandLine: {commandLine}");
                }
                return isMatch;
            }

            // --profile-directory 引数がない場合、Defaultプロファイルである可能性がある
            if (targetProfileId == "Default")
            {
                // --user-data-dir が指定されている場合は別ディレクトリなので除外
                bool isDefault = !commandLine.Contains("--user-data-dir") && 
                                 !commandLine.Contains("--single-argument");
                
                if (!isDefault)
                {
                    Logger.Info($"Default profile excluded due to args. CommandLine: {commandLine}");
                }
                return isDefault;
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

            foreach (var path in paths)
            {
                if (_fileSystem.FileExists(path))
                    return path;
            }

            return string.Empty;
        }
    }
}
