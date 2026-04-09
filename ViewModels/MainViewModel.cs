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
                // Pass a clone of all profiles
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
                System.Windows.MessageBox.Show($"Error opening settings: {ex.Message}\n{ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
            Helpers.Logger.Info("Initializing MainViewModel.");
            
            var iconService = new IconService(fileSystem);

            _discoveryService = discoveryService ?? new ProfileDiscoveryService(iconService, fileSystem);
            _launcherService = launcherService ?? new LauncherService(fileSystem);
            _settingsService = settingsService ?? new SettingsService(fileSystem);

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            Helpers.Logger.Info("Loading and merging profiles.");
            var settings = _settingsService.LoadSettings();
            var detected = _discoveryService.GetAvailableProfiles().ToList();
            Helpers.Logger.Info($"Detected {detected.Count} Chrome profiles.");

            var merged = new System.Collections.Generic.List<ProfileInfo>();

            // 1. Existing in settings
            foreach (var sProfile in settings.Profiles)
            {
                var dProfile = detected.FirstOrDefault(p => p.Id == sProfile.Id);
                if (dProfile != null)
                {
                    Helpers.Logger.Info($"Profile {sProfile.Id} matched. Preserving user settings (DisplayName: {sProfile.DisplayName}).");
                    sProfile.IconPath = dProfile.IconPath;
                    merged.Add(sProfile);
                    detected.Remove(dProfile);
                }
            }

            // 2. New profiles
            foreach (var dProfile in detected)
            {
                Helpers.Logger.Info($"New profile detected: {dProfile.Id}");
                int nextOrder = merged.Count > 0 ? merged.Max(p => p.Order) + 1 : 0;
                dProfile.Order = nextOrder;
                merged.Add(dProfile);
            }

            _allProfiles = merged.OrderBy(p => p.Order).ToList();
            Helpers.Logger.Info($"Final merged profile count: {_allProfiles.Count}");

            _settingsService.SaveSettings(new AppSettings { Profiles = _allProfiles });

            Profiles.Clear();
            foreach (var p in _allProfiles.Where(p => p.IsVisible))
            {
                Profiles.Add(p);
            }
        }
    }
}
