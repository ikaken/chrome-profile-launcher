using System;

namespace ChromeProfileLauncher.Helpers;

public static class FirstRunDetector
{
    public static bool IsFirstRun(string? velopackFirstRunEnv, bool settingsFileExists)
    {
        return string.Equals(velopackFirstRunEnv, "true", StringComparison.OrdinalIgnoreCase)
            || !settingsFileExists;
    }
}
