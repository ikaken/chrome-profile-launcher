using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;

namespace ChromeProfileLauncher.Helpers
{
    public static class LocalizationManager
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("ChromeProfileLauncher.Properties.Resources", typeof(LocalizationManager).Assembly);

        public static void SetLanguage(string languageCode)
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
        }

        public static string GetString(string key)
        {
            try
            {
                return _resourceManager.GetString(key) ?? key;
            }
            catch
            {
                return key;
            }
        }
    }
}
