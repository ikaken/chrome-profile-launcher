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
    }

    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);
    }
}
