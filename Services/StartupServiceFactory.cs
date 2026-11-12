using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher.Services;

public static class StartupServiceFactory
{
    public static IStartupService Create()
    {
        return PackagedAppHelper.IsPackaged ? new PackagedStartupService() : new StartupService();
    }
}
