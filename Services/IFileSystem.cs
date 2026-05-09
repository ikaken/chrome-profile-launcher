using System.Collections.Generic;
using System.IO;

namespace ChromeProfileLauncher.Services
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
        bool DirectoryExists(string path);
        DirectoryInfo CreateDirectory(string path);
        bool CanReadFile(string path);
    }

    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path, System.Text.Encoding.UTF8);
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents, System.Text.Encoding.UTF8);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);

        public bool CanReadFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                using (var stream = File.OpenRead(path))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
