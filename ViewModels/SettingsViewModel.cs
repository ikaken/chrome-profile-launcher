using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChromeProfileLauncher.Helpers;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;

namespace ChromeProfileLauncher.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IStartupService _startupService;
        private readonly IUpdateService _updateService;
        public ObservableCollection<ProfileInfo> Profiles { get; } = new();

        public bool LaunchAtStartup
        {
            get => _startupService.IsRegistered();
            set
            {
                if (value) _startupService.Register();
                else _startupService.Unregister();
                OnPropertyChanged();
            }
        }

        private bool _enableTaskTray;
        public bool EnableTaskTray
        {
            get => _enableTaskTray;
            set { if (_enableTaskTray != value) { _enableTaskTray = value; OnPropertyChanged(); } }
        }

        private string _language = "ja-JP";
        public string Language
        {
            get => _language;
            set { if (_language != value) { _language = value; OnPropertyChanged(); } }
        }

        public string CurrentVersion => _updateService.GetCurrentVersion();

        private bool _isCheckingForUpdates;
        public bool IsCheckingForUpdates
        {
            get => _isCheckingForUpdates;
            set { if (_isCheckingForUpdates != value) { _isCheckingForUpdates = value; OnPropertyChanged(); } }
        }

        private ICommand? _checkForUpdatesCommand;
        public ICommand CheckForUpdatesCommand => _checkForUpdatesCommand ??= new RelayCommand(async _ =>
        {
            try
            {
                IsCheckingForUpdates = true;
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                
                if (updateInfo != null)
                {
                    var result = System.Windows.MessageBox.Show(
                        $"新しいバージョン ({updateInfo.TargetFullRelease.Version}) が利用可能です。ダウンロードしますか？",
                        "アップデートの通知",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        await _updateService.DownloadUpdateAsync(updateInfo);
                        
                        var restartResult = System.Windows.MessageBox.Show(
                            "ダウンロードが完了しました。今すぐ適用して再起動しますか？",
                            "アップデートの準備完了",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Question);

                        if (restartResult == System.Windows.MessageBoxResult.Yes)
                        {
                            _updateService.ApplyUpdateAndRestart(updateInfo);
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("現在、最新のバージョンを使用しています。", "アップデートの確認");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Manual update check failed.", ex);
                System.Windows.MessageBox.Show($"アップデートの確認中にエラーが発生しました: {ex.Message}");
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        });

        private ICommand? _openUrlCommand;
        public ICommand OpenUrlCommand => _openUrlCommand ??= new RelayCommand(url =>
        {
            if (url is string urlString && !string.IsNullOrEmpty(urlString))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = urlString,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to open URL: {urlString}", ex);
                    System.Windows.MessageBox.Show($"リンクを開けませんでした: {ex.Message}");
                }
            }
        });

        private double _windowTop;
        public double WindowTop
        {
            get => _windowTop;
            set { if (_windowTop != value) { _windowTop = value; OnPropertyChanged(); } }
        }

        private double _windowLeft;
        public double WindowLeft
        {
            get => _windowLeft;
            set { if (_windowLeft != value) { _windowLeft = value; OnPropertyChanged(); } }
        }

        private double _windowWidth;
        public double WindowWidth
        {
            get => _windowWidth;
            set { if (_windowWidth != value) { _windowWidth = value; OnPropertyChanged(); } }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get => _windowHeight;
            set { if (_windowHeight != value) { _windowHeight = value; OnPropertyChanged(); } }
        }

        private ICommand? _saveCommand;
        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(p =>
        {
            try
            {
                SaveWindowSettings();
                var settings = _settingsService.LoadSettings();
                settings.Profiles = Profiles.ToList();
                settings.EnableTaskTray = EnableTaskTray;
                settings.Language = Language; // 言語設定を保存
                _settingsService.SaveSettings(settings);
                
                // 言語設定を即時反映
                LocalizationManager.SetLanguage(Language);
                
                if (p is System.Windows.Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format(LocalizationManager.GetString("ErrorDuringSave"), ex.Message), LocalizationManager.GetString("Error"));
            }
        });

        public void SaveWindowSettings()
        {
            var settings = _settingsService.LoadSettings() ?? new AppSettings();

            settings.SettingsWindowTop = WindowTop;
            settings.SettingsWindowLeft = WindowLeft;
            settings.SettingsWindowWidth = WindowWidth;
            settings.SettingsWindowHeight = WindowHeight;
            _settingsService.SaveSettings(settings);
        }

        private ICommand? _openFolderCommand;
        public ICommand OpenFolderCommand => _openFolderCommand ??= new RelayCommand(p =>
        {
            if (p is ProfileInfo profile)
            {
                try
                {
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var userDataPath = System.IO.Path.Combine(localAppData, "Google", "Chrome", "User Data");
                    var profilePath = System.IO.Path.Combine(userDataPath, profile.Id);

                    if (System.IO.Directory.Exists(profilePath))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", profilePath);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Folder not found: {profilePath}");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening folder: {ex.Message}");
                }
            }
        });

        private ICommand? _moveUpCommand;
        public ICommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(p =>
        {
            if (p is ProfileInfo profile)
            {
                var index = Profiles.IndexOf(profile);
                if (index > 0)
                {
                    Profiles.RemoveAt(index);
                    Profiles.Insert(index - 1, profile);
                    UpdateOrders();
                }
            }
        });

        private ICommand? _moveDownCommand;
        public ICommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(p =>
        {
            if (p is ProfileInfo profile)
            {
                var index = Profiles.IndexOf(profile);
                if (index < Profiles.Count - 1)
                {
                    Profiles.RemoveAt(index);
                    Profiles.Insert(index + 1, profile);
                    UpdateOrders();
                }
            }
        });

        public SettingsViewModel() : this(null, new SettingsService(new FileSystem()), null) { }

        public SettingsViewModel(System.Collections.Generic.IEnumerable<ProfileInfo>? initialProfiles)
            : this(initialProfiles, new SettingsService(new FileSystem()), null) { }

        public SettingsViewModel(System.Collections.Generic.IEnumerable<ProfileInfo>? initialProfiles, ISettingsService settingsService, IUpdateService? updateService = null)
        {
            _settingsService = settingsService;
            _startupService = new StartupService();
            _updateService = updateService ?? new UpdateService();
            
            // Load window settings
            var settings = _settingsService.LoadSettings();
            WindowTop = settings?.SettingsWindowTop ?? 200;
            WindowLeft = settings?.SettingsWindowLeft ?? 200;
            WindowWidth = settings?.SettingsWindowWidth ?? 500;
            WindowHeight = settings?.SettingsWindowHeight ?? 550;
            EnableTaskTray = settings?.EnableTaskTray ?? false;
            Language = settings?.Language ?? "auto";
            
            if (initialProfiles != null)
            {
                foreach (var p in initialProfiles)
                {
                    Profiles.Add(p);
                }
            }
            else
            {
                // Fallback to loading from disk if nothing passed
                try
                {
                    if (settings?.Profiles != null)
                    {
                        foreach (var p in settings.Profiles.OrderBy(p => p.Order))
                        {
                            Profiles.Add(p);
                        }
                    }
                }
                catch { /* Ignore */ }
            }
        }

        public void MoveProfile(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex && oldIndex >= 0 && oldIndex < Profiles.Count && newIndex >= 0 && newIndex < Profiles.Count)
            {
                var profile = Profiles[oldIndex];
                Profiles.RemoveAt(oldIndex);
                Profiles.Insert(newIndex, profile);
                UpdateOrders();
            }
        }

        private void UpdateOrders()
        {
            for (int i = 0; i < Profiles.Count; i++)
            {
                Profiles[i].Order = i;
            }
        }
    }
}
