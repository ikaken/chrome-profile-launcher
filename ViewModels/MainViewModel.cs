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

        private System.Collections.Generic.List<ProfileInfo> _allProfiles = new();
        public ObservableCollection<ProfileInfo> Profiles { get; } = new();
        
        private bool _isDimmed;
        public bool IsDimmed
        {
            get => _isDimmed;
            set { if (_isDimmed != value) { _isDimmed = value; OnPropertyChanged(); } }
        }

        private ICommand? _launchCommand;
        public ICommand LaunchCommand => _launchCommand ??= new RelayCommand(p =>
        {
            if (p is ProfileInfo profile)
            {
                _launcherService.LaunchOrFocus(profile);
            }
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

                var vm = new SettingsViewModel(clone);
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

        public MainViewModel() : this(new FileSystem(), null, null, null)
        {
        }

        public MainViewModel(
            IFileSystem fileSystem, 
            IProfileDiscoveryService? discoveryService = null, 
            ILauncherService? launcherService = null, 
            ISettingsService? settingsService = null)
        {
            Logger.Info("Initializing MainViewModel.");
            
            _discoveryService = discoveryService ?? new ProfileDiscoveryService(new IconService(fileSystem), fileSystem);
            _launcherService = launcherService ?? new LauncherService(fileSystem);
            _settingsService = settingsService ?? new SettingsService(fileSystem);

            LoadProfiles();
        }

        private double? _windowTop;
        public double? WindowTop
        {
            get => _windowTop;
            set { if (_windowTop != value) { _windowTop = value; OnPropertyChanged(); } }
        }

        private double? _windowLeft;
        public double? WindowLeft
        {
            get => _windowLeft;
            set { if (_windowLeft != value) { _windowLeft = value; OnPropertyChanged(); } }
        }

        private double? _windowWidth;
        public double? WindowWidth
        {
            get => _windowWidth;
            set { if (_windowWidth != value) { _windowWidth = value; OnPropertyChanged(); } }
        }

        private double? _windowHeight;
        public double? WindowHeight
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
            
            // Load window settings
            WindowTop = settings.WindowTop ?? 100; // Default if null
            WindowLeft = settings.WindowLeft ?? 100;
            WindowWidth = settings.WindowWidth ?? 420;
            WindowHeight = settings.WindowHeight ?? 500;
            WindowState = settings.IsMaximized ? System.Windows.WindowState.Maximized : System.Windows.WindowState.Normal;

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
            _settingsService.SaveSettings(new AppSettings { Profiles = _allProfiles });

            Profiles.Clear();
            foreach (var p in _allProfiles.Where(p => p.IsVisible))
            {
                Profiles.Add(p);
            }
        }
    }
}
