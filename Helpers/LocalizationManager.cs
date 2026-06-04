using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;

namespace ChromeProfileLauncher.Helpers
{
    public static class LocalizationManager
    {
        private static ResourceManager? _resourceManager;

        private static ResourceManager GetResourceManager()
        {
            if (_resourceManager == null)
            {
                try
                {
                    _resourceManager = new ResourceManager("ChromeProfileLauncher.Properties.Resources", typeof(LocalizationManager).Assembly);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to initialize ResourceManager", ex);
                    throw;
                }
            }
            return _resourceManager;
        }

        public static event EventHandler? LanguageChanged;

        public static void SetLanguage(string languageCode)
        {
            try
            {
                CultureInfo culture;
                if (string.IsNullOrEmpty(languageCode) || languageCode.ToLower() == "auto")
                {
                    culture = CultureInfo.CurrentUICulture;
                }
                else
                {
                    try
                    {
                        culture = new CultureInfo(languageCode);
                    }
                    catch
                    {
                        culture = CultureInfo.CurrentUICulture;
                    }
                }

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                Logger.Info($"Language set to: {culture.Name}");
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to set language", ex);
            }
        }

        public static string GetString(string key)
        {
            try
            {
                var rm = GetResourceManager();
                var value = rm.GetString(key);
                if (value == null)
                {
                    Logger.Info($"Resource key not found: {key}");
                    return $"!{key}!";
                }
                return value;
            }
            catch (Exception ex)
            {
                // ここでエラーが出るとループする可能性があるので注意（LoggerがGetStringを呼ぶ場合など）
                // ただし現在の Logger は呼んでいない。
                System.Diagnostics.Debug.WriteLine($"Error getting string for {key}: {ex.Message}");
                return key;
            }
        }
    }
}
