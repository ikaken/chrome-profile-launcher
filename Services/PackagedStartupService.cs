using System;
using System.Runtime.Versioning;
using ChromeProfileLauncher.Helpers;
using Windows.ApplicationModel;

namespace ChromeProfileLauncher.Services;

[SupportedOSPlatform("windows10.0.17763.0")]
public class PackagedStartupService : IStartupService
{
    private const string TaskId = "ChromeProfileLauncherStartup";

    private static StartupTask? GetStartupTask()
    {
        return StartupTask.GetAsync(TaskId).AsTask().GetAwaiter().GetResult();
    }

    public bool IsRegistered()
    {
        try
        {
            var task = GetStartupTask();
            return task != null && task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to get startup task state.", ex);
            return false;
        }
    }

    public void Register()
    {
        try
        {
            var task = GetStartupTask();
            if (task == null) return;

            switch (task.State)
            {
                case StartupTaskState.Disabled:
                    task.RequestEnableAsync().AsTask().GetAwaiter().GetResult();
                    break;
                case StartupTaskState.DisabledByUser:
                    Logger.Info("Startup task is disabled by user. Enable it in Task Manager.");
                    break;
                case StartupTaskState.DisabledByPolicy:
                    Logger.Info("Startup task is disabled by policy.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to enable startup task.", ex);
        }
    }

    public void Unregister()
    {
        try
        {
            var task = GetStartupTask();
            task?.Disable();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to disable startup task.", ex);
        }
    }
}
