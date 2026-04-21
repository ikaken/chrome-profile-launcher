using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ChromeProfileLauncher.Models;

namespace ChromeProfileLauncher.Services
{
    public class AppSettings
    {
        public List<ProfileInfo> Profiles { get; set; } = new();
        public double? WindowTop { get; set; }
        public double? WindowLeft { get; set; }
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }
        public bool IsMaximized { get; set; }

        public double? SettingsWindowTop { get; set; }
        public double? SettingsWindowLeft { get; set; }
        public double? SettingsWindowWidth { get; set; }
        public double? SettingsWindowHeight { get; set; }
    }

    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
    }

    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly IFileSystem _fileSystem;

        public SettingsService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "ChromeProfileLauncher");
            if (!_fileSystem.DirectoryExists(dir)) _fileSystem.CreateDirectory(dir);
            _settingsPath = Path.Combine(dir, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            Helpers.Logger.Info($"Loading settings from: {_settingsPath}");
            if (!_fileSystem.FileExists(_settingsPath))
            {
                Helpers.Logger.Info("Settings file not found, returning default.");
                return new AppSettings();
            }

            try
            {
                var json = _fileSystem.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                Helpers.Logger.Info($"Successfully loaded {settings.Profiles.Count} profiles from settings.");
                return settings;
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("Failed to load settings.", ex);
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                Helpers.Logger.Info($"Saving {settings.Profiles.Count} profiles to: {_settingsPath}");
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                _fileSystem.WriteAllText(_settingsPath, json);
                Helpers.Logger.Info("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("Failed to save settings.", ex);
                System.Windows.MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
