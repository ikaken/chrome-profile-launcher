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
    private readonly Lazy<UpdateManager?> _updateManagerLazy;
    private readonly bool _isPackaged;

    public UpdateService(bool? isPackaged = null)
    {
        _isPackaged = isPackaged ?? PackagedAppHelper.IsPackaged;
        _updateManagerLazy = new Lazy<UpdateManager?>(() =>
        {
            try
            {
                return new UpdateManager(new GithubSource(_repoUrl, null, false));
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateManager is not available.", ex);
                return null;
            }
        });
    }

    private UpdateManager? UpdateManager => _isPackaged ? null : _updateManagerLazy.Value;

    public bool IsUpdateSupported => !_isPackaged;

    public string GetCurrentVersion()
    {
        var version = UpdateManager?.CurrentVersion?.ToString();
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
        if (UpdateManager == null)
        {
            Logger.Info("Update manager is not available. Skipping update check.");
            return null;
        }
        try
        {
            Logger.Info("Checking for updates...");
            var info = await UpdateManager.CheckForUpdatesAsync();
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
            if (UpdateManager == null) throw new InvalidOperationException("Update manager is not available.");
            await UpdateManager.DownloadUpdatesAsync(updateInfo);
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
            if (UpdateManager == null) throw new InvalidOperationException("Update manager is not available.");
            UpdateManager.ApplyUpdatesAndRestart(updateInfo.TargetFullRelease);
        }
        catch (Exception ex)
        {
            Logger.Error("Error applying update.", ex);
            throw;
        }
    }
}
