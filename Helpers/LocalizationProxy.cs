using System.ComponentModel;

namespace ChromeProfileLauncher.Helpers
{
    public class LocalizationProxy : INotifyPropertyChanged
    {
        public LocalizationProxy()
        {
            LocalizationManager.LanguageChanged += (s, e) => Refresh();
        }

        public string AppTitle => LocalizationManager.GetString("AppTitle");
        public string Settings => LocalizationManager.GetString("Settings");
        public string LauncherSettings => LocalizationManager.GetString("LauncherSettings");
        public string ProfileManagement => LocalizationManager.GetString("ProfileManagement");
        public string ReloadProfiles => LocalizationManager.GetString("ReloadProfiles");
        public string AppSettings => LocalizationManager.GetString("AppSettings");
        public string LaunchAtStartup => LocalizationManager.GetString("LaunchAtStartup");
        public string LaunchAtStartupDescription => LocalizationManager.GetString("LaunchAtStartupDescription");
        public string EnableTaskTray => LocalizationManager.GetString("EnableTaskTray");
        public string EnableTaskTrayDescription => LocalizationManager.GetString("EnableTaskTrayDescription");
        public string AppUpdate => LocalizationManager.GetString("AppUpdate");
        public string EnableAutoUpdate => LocalizationManager.GetString("EnableAutoUpdate");
        public string EnableAutoUpdateDescription => LocalizationManager.GetString("EnableAutoUpdateDescription");
        public string CurrentVersion => LocalizationManager.GetString("CurrentVersion");
        public string CheckForUpdates => LocalizationManager.GetString("CheckForUpdates");
        public string Cancel => LocalizationManager.GetString("Cancel");
        public string Save => LocalizationManager.GetString("Save");
        public string Language => LocalizationManager.GetString("Language");
        public string LanguageDescription => LocalizationManager.GetString("LanguageDescription");
        public string LanguageJapanese => LocalizationManager.GetString("LanguageJapanese");
        public string LanguageEnglish => LocalizationManager.GetString("LanguageEnglish");
        public string Error => LocalizationManager.GetString("Error");
        public string Success => LocalizationManager.GetString("Success");
        public string DonationTitle => LocalizationManager.GetString("DonationTitle");
        public string DonationDescription => LocalizationManager.GetString("DonationDescription");
        public string DonationGitHub => LocalizationManager.GetString("DonationGitHub");
        public string DonationKofi => LocalizationManager.GetString("DonationKofi");
        public string ErrorDuringSave => LocalizationManager.GetString("ErrorDuringSave");
        public string HotkeyNone => LocalizationManager.GetString("HotkeyNone");
        public string HotkeySettings => LocalizationManager.GetString("HotkeySettings");
        public string HotkeySettingsDescription => LocalizationManager.GetString("HotkeySettingsDescription");
        public string HotkeyAlt => LocalizationManager.GetString("HotkeyAlt");
        public string HotkeyCtrl => LocalizationManager.GetString("HotkeyCtrl");
        public string HotkeyShift => LocalizationManager.GetString("HotkeyShift");
        public string FirstRunTitle => LocalizationManager.GetString("FirstRunTitle");
        public string FirstRunHeading => LocalizationManager.GetString("FirstRunHeading");
        public string FirstRunDescription => LocalizationManager.GetString("FirstRunDescription");
        public string FirstRunContinue => LocalizationManager.GetString("FirstRunContinue");

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
