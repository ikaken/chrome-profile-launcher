using System;
using System.IO;
using ChromeProfileLauncher.Services;
using Moq;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class IconServiceTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly IconService _service;
        private readonly string _userDataPath;

        public IconServiceTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _service = new IconService(_fileSystemMock.Object);

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _userDataPath = Path.Combine(localAppData, "Google", "Chrome", "User Data");
        }

        [Fact]
        public void GetIconPath_ShouldReturnPngPath_WhenPngIsReadable()
        {
            // Arrange
            var profileId = "Default";
            var expectedPicPath = Path.Combine(_userDataPath, profileId, "Google Profile Picture.png");
            _fileSystemMock.Setup(f => f.CanReadFile(expectedPicPath)).Returns(true);

            // Act
            var result = _service.GetIconPath(profileId);

            // Assert
            result.Should().Be(expectedPicPath);
        }

        [Fact]
        public void GetIconPath_ShouldReturnIcoPath_WhenPngIsNotReadable()
        {
            // Arrange
            var profileId = "Default";
            var pngPath = Path.Combine(_userDataPath, profileId, "Google Profile Picture.png");
            var icoPath = Path.Combine(_userDataPath, profileId, "Google Profile.ico");
            _fileSystemMock.Setup(f => f.CanReadFile(pngPath)).Returns(false);
            _fileSystemMock.Setup(f => f.CanReadFile(icoPath)).Returns(true);

            // Act
            var result = _service.GetIconPath(profileId);

            // Assert
            result.Should().Be(icoPath);
        }

        [Fact]
        public void GetIconPath_ShouldReturnChromePath_WhenNoIconAvailable()
        {
            // Arrange
            _fileSystemMock.Setup(f => f.CanReadFile(It.IsAny<string>())).Returns(false);
            _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            var result = _service.GetIconPath("NonExistent");

            // Assert - chrome.exe も見つからない場合は空文字列
            result.Should().BeEmpty();
        }
    }
}
