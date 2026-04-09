using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ChromeProfileLauncher.Models;

namespace ChromeProfileLauncher.Services
{
    public interface IProfileDiscoveryService
    {
        IEnumerable<ProfileInfo> GetAvailableProfiles();
    }

    public class ProfileDiscoveryService : IProfileDiscoveryService
    {
        private readonly IIconService _iconService;
        private readonly IFileSystem _fileSystem;

        public ProfileDiscoveryService(IIconService iconService, IFileSystem fileSystem)
        {
            _iconService = iconService;
            _fileSystem = fileSystem;
        }

        public IEnumerable<ProfileInfo> GetAvailableProfiles()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var userDataPath = Path.Combine(localAppData, "Google", "Chrome", "User Data");
            var localStatePath = Path.Combine(userDataPath, "Local State");

            var profiles = new List<ProfileInfo>();

            if (!_fileSystem.FileExists(localStatePath))
                return profiles;

            try
            {
                var jsonContent = _fileSystem.ReadAllText(localStatePath);
                using var doc = JsonDocument.Parse(jsonContent);
                if (doc.RootElement.TryGetProperty("profile", out var profileElement) &&
                    profileElement.TryGetProperty("info_cache", out var infoCacheElement))
                {
                    int order = 0;
                    foreach (var profileProp in infoCacheElement.EnumerateObject())
                    {
                        var profileId = profileProp.Name;
                        var name = profileId; // Fallback

                        if (profileProp.Value.TryGetProperty("name", out var nameProp))
                        {
                            name = nameProp.GetString() ?? profileId;
                        }

                        profiles.Add(new ProfileInfo
                        {
                            Id = profileId,
                            DisplayName = name,
                            Order = order++,
                            IsVisible = true,
                            IconPath = _iconService.GetIconPath(profileId)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Add logging
                Console.WriteLine($"Error parsing Local State: {ex.Message}");
            }

            return profiles;
        }
    }
}
