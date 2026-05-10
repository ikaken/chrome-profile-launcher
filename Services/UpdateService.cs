using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher.Services;

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task DownloadUpdateAsync(UpdateInfo updateInfo);
    void ApplyUpdateAndRestart(UpdateInfo updateInfo);
    string GetCurrentVersion();
}

public class UpdateService : IUpdateService
{
    private readonly string _repoUrl = "https://github.com/ikaken/chrome-profile-launcher";
    private readonly UpdateManager _updateManager;

    public UpdateService()
    {
        _updateManager = new UpdateManager(new GithubSource(_repoUrl, null, false));
    }

    public string GetCurrentVersion()
    {
        return _updateManager.CurrentVersion?.ToString() ?? "0.0.0";
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            Logger.Info("Checking for updates...");
            var info = await _updateManager.CheckForUpdatesAsync();
            if (info != null)
            {
                Logger.Info($"Update found: {info.TargetFullRelease.Version}");
            }
            else
            {
                Logger.Info("No updates found.");
            }
            return info;
        }
        catch (Exception ex)
        {
            Logger.Error("Error checking for updates.", ex);
            return null;
        }
    }

    public async Task DownloadUpdateAsync(UpdateInfo updateInfo)
    {
        try
        {
            Logger.Info($"Downloading update: {updateInfo.TargetFullRelease.Version}...");
            await _updateManager.DownloadUpdatesAsync(updateInfo);
            Logger.Info("Update downloaded successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error("Error downloading update.", ex);
            throw;
        }
    }

    public void ApplyUpdateAndRestart(UpdateInfo updateInfo)
    {
        try
        {
            Logger.Info("Applying update and restarting...");
            _updateManager.ApplyUpdatesAndRestart(updateInfo.TargetFullRelease);
        }
        catch (Exception ex)
        {
            Logger.Error("Error applying update.", ex);
            throw;
        }
    }
}
