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
        public ObservableCollection<ProfileInfo> Profiles { get; } = new();

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

        private ICommand? _saveCommand;
        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(p =>
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                settings.Profiles = Profiles.ToList();
                _settingsService.SaveSettings(settings);
                
                if (p is System.Windows.Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error during save: {ex.Message}");
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

        public SettingsViewModel() : this(null, new SettingsService(new FileSystem())) { }

        public SettingsViewModel(System.Collections.Generic.IEnumerable<ProfileInfo>? initialProfiles)
            : this(initialProfiles, new SettingsService(new FileSystem())) { }

        public SettingsViewModel(System.Collections.Generic.IEnumerable<ProfileInfo>? initialProfiles, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            // Load window settings
            var settings = _settingsService.LoadSettings();
            WindowTop = settings?.SettingsWindowTop ?? 200;
            WindowLeft = settings?.SettingsWindowLeft ?? 200;
            WindowWidth = settings?.SettingsWindowWidth ?? 500;
            WindowHeight = settings?.SettingsWindowHeight ?? 450;
            
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
