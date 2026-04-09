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
        public void GetIconPath_ShouldReturnPath_WhenFileExists()
        {
            // Arrange
            var profileId = "Default";
            var expectedPicPath = Path.Combine(_userDataPath, profileId, "Google Profile Picture.png");
            _fileSystemMock.Setup(f => f.FileExists(expectedPicPath)).Returns(true);

            // Act
            var result = _service.GetIconPath(profileId);

            // Assert
            result.Should().Be(expectedPicPath);
        }

        [Fact]
        public void GetIconPath_ShouldReturnEmpty_WhenFileDoesNotExist()
        {
            // Arrange
            _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            var result = _service.GetIconPath("NonExistent");

            // Assert
            result.Should().BeEmpty();
        }
    }
}
