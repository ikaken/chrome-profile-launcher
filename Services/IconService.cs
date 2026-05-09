using System;
using System.IO;
using System.Windows.Media.Imaging;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher.Services
{
    public interface IIconService
    {
        string GetIconPath(string profileId);
    }

    public class IconService : IIconService
    {
        private readonly string _userDataPath;
        private readonly IFileSystem _fileSystem;

        public IconService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _userDataPath = Path.Combine(localAppData, "Google", "Chrome", "User Data");
        }

        public string GetIconPath(string profileId)
        {
            // 1. Check Google Profile Picture.png
            var profilePath = Path.Combine(_userDataPath, profileId);
            var picPath = Path.Combine(profilePath, "Google Profile Picture.png");

            if (_fileSystem.CanReadFile(picPath))
            {
                return picPath;
            }
            
            // 2. Fallback to Google Profile.ico
            var icoPath = Path.Combine(profilePath, "Google Profile.ico");
            if (_fileSystem.CanReadFile(icoPath))
            {
                return icoPath;
            }

            // 3. Final Fallback to Chrome icon (EXE)
            return ChromePathHelper.GetChromePath(_fileSystem);
        }
    }
}
