using System;
using System.Windows.Input;
using ChromeProfileLauncher.Helpers;
using ChromeProfileLauncher.Services;

namespace ChromeProfileLauncher.ViewModels
{
    public class FirstRunSetupViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IStartupService _startupService;
        private readonly string _language;
        private bool _launchAtStartup;
        private bool _enableTaskTray;
        private ICommand? _saveCommand;

        public FirstRunSetupViewModel(ISettingsService settingsService, IStartupService startupService, string language)
        {
            _settingsService = settingsService;
            _startupService = startupService;
            _language = language;
        }

        public event EventHandler? Completed;

        public bool LaunchAtStartup
        {
            get => _launchAtStartup;
            set
            {
                if (_launchAtStartup == value) return;
                _launchAtStartup = value;
                OnPropertyChanged();
            }
        }

        public bool EnableTaskTray
        {
            get => _enableTaskTray;
            set
            {
                if (_enableTaskTray == value) return;
                _enableTaskTray = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(_ => Save());

        private void Save()
        {
            try
            {
                if (LaunchAtStartup) _startupService.Register();
                else _startupService.Unregister();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to apply startup registration during first-run setup.", ex);
            }

            try
            {
                var settings = _settingsService.LoadSettings();
                settings.EnableTaskTray = EnableTaskTray;
                settings.Language = _language;
                _settingsService.SaveSettings(settings);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save first-run settings.", ex);
            }

            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}
