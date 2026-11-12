using System;
using System.Collections.Generic;
using System.Reflection;
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
    bool IsUpdateSupported { get; }
}

public class UpdateService : IUpdateService
{
    private readonly string _repoUrl = "https://github.com/ikaken/chrome-profile-launcher";
    private readonly UpdateManager _updateManager;
    private readonly bool _isPackaged;

    public UpdateService(bool? isPackaged = null)
    {
        _isPackaged = isPackaged ?? PackagedAppHelper.IsPackaged;
        _updateManager = new UpdateManager(new GithubSource(_repoUrl, null, false));
    }

    public bool IsUpdateSupported => !_isPackaged;

    public string GetCurrentVersion()
    {
        var version = _updateManager.CurrentVersion?.ToString();
        if (!string.IsNullOrEmpty(version)) return version;

        var informationalVersion = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (!string.IsNullOrEmpty(informationalVersion))
        {
            var plusIndex = informationalVersion.IndexOf('+');
            return plusIndex >= 0 ? informationalVersion.Substring(0, plusIndex) : informationalVersion;
        }

        return Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        if (_isPackaged)
        {
            Logger.Info("Running as packaged app. Updates are handled by Microsoft Store.");
            return null;
        }
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
