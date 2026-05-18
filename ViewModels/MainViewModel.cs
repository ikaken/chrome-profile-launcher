using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChromeProfileLauncher.Helpers;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;

namespace ChromeProfileLauncher.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IProfileDiscoveryService _discoveryService;
        private readonly ILauncherService _launcherService;
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;

        private System.Collections.Generic.List<ProfileInfo> _allProfiles = new();
        public ObservableCollection<ProfileInfo> Profiles { get; } = new();
        
        private bool _isDimmed;
        public bool IsDimmed
        {
            get => _isDimmed;
            set { if (_isDimmed != value) { _isDimmed = value; OnPropertyChanged(); } }
        }

        private System.Windows.Visibility _visibility = System.Windows.Visibility.Visible;
        public System.Windows.Visibility Visibility
        {
            get => _visibility;
            set { if (_visibility != value) { _visibility = value; OnPropertyChanged(); } }
        }

        private ICommand? _launchCommand;
        public ICommand LaunchCommand => _launchCommand ??= new RelayCommand(p =>
        {
            if (p is ProfileInfo profile)
            {
                _launcherService.LaunchOrFocus(profile);
            }
        });

        private ICommand? _showWindowCommand;
        public ICommand ShowWindowCommand => _showWindowCommand ??= new RelayCommand(_ =>
        {
            Visibility = System.Windows.Visibility.Visible;
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                WindowState = System.Windows.WindowState.Normal;
            }
            System.Windows.Application.Current.MainWindow?.Activate();
            System.Windows.Application.Current.MainWindow?.Focus();
        });

        private ICommand? _exitApplicationCommand;
        public ICommand ExitApplicationCommand => _exitApplicationCommand ??= new RelayCommand(_ =>
        {
            // Set a flag or just shutdown. To bypass tray logic on exit, we might need a flag.
            // For now, let's just shutdown.
            System.Windows.Application.Current.Shutdown();
        });

        private ICommand? _settingsCommand;
        public ICommand SettingsCommand => _settingsCommand ??= new RelayCommand(_ =>
        {
            try
            {
                var clone = _allProfiles.Select(p => new ProfileInfo 
                { 
                    Id = p.Id, 
                    DisplayName = p.DisplayName, 
                    IsVisible = p.IsVisible, 
                    Order = p.Order, 
                    IconPath = p.IconPath
                }).ToList();

                var vm = new SettingsViewModel(clone, _settingsService, _updateService);
                var settingsWin = new SettingsWindow();
                settingsWin.DataContext = vm;
                settingsWin.Owner = System.Windows.Application.Current.MainWindow;
                
                IsDimmed = true;
                try
                {
                    if (settingsWin.ShowDialog() == true)
                    {
                        LoadProfiles();
                    }
                }
                finally
                {
                    IsDimmed = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening settings: {ex.Message}");
            }
        });

        public MainViewModel() : this(new FileSystem(), null, null, null, null)
        {
        }

        public MainViewModel(
            IFileSystem fileSystem, 
            IProfileDiscoveryService? discoveryService = null, 
            ILauncherService? launcherService = null, 
            ISettingsService? settingsService = null,
            IUpdateService? updateService = null)
        {
            Logger.Info("Initializing MainViewModel.");
            
            _discoveryService = discoveryService ?? new ProfileDiscoveryService(new IconService(fileSystem), fileSystem);
            _launcherService = launcherService ?? new LauncherService(fileSystem);
            _settingsService = settingsService ?? new SettingsService(fileSystem);
            _updateService = updateService ?? new UpdateService();

            LoadProfiles();
            CheckForUpdatesAsync().ConfigureAwait(false);
        }

        private async System.Threading.Tasks.Task CheckForUpdatesAsync()
        {
            try
            {
                // UIスレッドをブロックしないよう少し遅延させる（UX配慮）
                await System.Threading.Tasks.Task.Delay(3000);
                
                var updateInfo = await _updateService.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    Logger.Info($"Update available: {updateInfo.TargetFullRelease.Version}");
                    
                    // アップデート通知ダイアログを表示（簡易版としてMessageBoxを使用）
                    // 実際にはもっとリッチなUIが望ましい
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        var result = System.Windows.MessageBox.Show(
                            $"新しいバージョン ({updateInfo.TargetFullRelease.Version}) が利用可能です。アップデートをダウンロードしますか？",
                            "アップデートの通知",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Information);

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            try
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
                            catch (Exception ex)
                            {
                                Logger.Error("Update download failed.", ex);
                                System.Windows.MessageBox.Show("アップデートのダウンロードに失敗しました。");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during update check.", ex);
            }
        }

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

        private bool _isMaximized;
        public bool IsMaximized
        {
            get => _isMaximized;
            set { if (_isMaximized != value) { _isMaximized = value; OnPropertyChanged(); } }
        }

        private System.Windows.WindowState _windowState;
        public System.Windows.WindowState WindowState
        {
            get => _windowState;
            set 
            { 
                if (_windowState != value) 
                { 
                    _windowState = value; 
                    IsMaximized = (_windowState == System.Windows.WindowState.Maximized);
                    OnPropertyChanged(); 
                } 
            }
        }

        public void SaveWindowSettings()
        {
            var settings = _settingsService.LoadSettings();
            
            // Only save position and size if not maximized/minimized
            if (WindowState == System.Windows.WindowState.Normal)
            {
                settings.WindowTop = WindowTop;
                settings.WindowLeft = WindowLeft;
                settings.WindowWidth = WindowWidth;
                settings.WindowHeight = WindowHeight;
            }
            
            settings.IsMaximized = (WindowState == System.Windows.WindowState.Maximized);
            settings.Profiles = _allProfiles; // Preserve profiles
            _settingsService.SaveSettings(settings);
        }

        private void LoadProfiles()
        {
            var settings = _settingsService.LoadSettings();
            
            var detected = _discoveryService.GetAvailableProfiles().ToList();

            var merged = new System.Collections.Generic.List<ProfileInfo>();

            foreach (var sProfile in settings.Profiles)
            {
                var dProfile = detected.FirstOrDefault(p => p.Id == sProfile.Id);
                if (dProfile != null)
                {
                    sProfile.IconPath = dProfile.IconPath;
                    merged.Add(sProfile);
                    detected.Remove(dProfile);
                }
            }

            foreach (var dProfile in detected)
            {
                int nextOrder = merged.Count > 0 ? merged.Max(p => p.Order) + 1 : 0;
                dProfile.Order = nextOrder;
                merged.Add(dProfile);
            }

            _allProfiles = merged.OrderBy(p => p.Order).ToList();
            
            // Preserve window settings when saving profiles
            settings.Profiles = _allProfiles;
            _settingsService.SaveSettings(settings);

            Profiles.Clear();
            foreach (var p in _allProfiles.Where(p => p.IsVisible))
            {
                Profiles.Add(p);
            }
        }
    }
}
