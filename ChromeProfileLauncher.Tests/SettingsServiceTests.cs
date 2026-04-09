using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;
using Moq;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class SettingsServiceTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly SettingsService _service;
        private readonly string _settingsPath;

        public SettingsServiceTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _settingsPath = System.IO.Path.Combine(appData, "ChromeProfileLauncher", "settings.json");

            // Setup Directory check & creation
            _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);

            _service = new SettingsService(_fileSystemMock.Object);
        }

        [Fact]
        public void LoadSettings_ShouldReturnEmpty_WhenFileDoesNotExist()
        {
            // Arrange
            _fileSystemMock.Setup(f => f.FileExists(_settingsPath)).Returns(false);

            // Act
            var results = _service.LoadSettings();

            // Assert
            results.Profiles.Should().BeEmpty();
        }

        [Fact]
        public void LoadSettings_ShouldReturnSettings_WhenFileExists()
        {
            // Arrange
            var settings = new AppSettings
            {
                Profiles = new List<ProfileInfo>
                {
                    new ProfileInfo { Id = "Default", DisplayName = "My Profile" }
                }
            };
            var json = JsonSerializer.Serialize(settings);

            _fileSystemMock.Setup(f => f.FileExists(_settingsPath)).Returns(true);
            _fileSystemMock.Setup(f => f.ReadAllText(_settingsPath)).Returns(json);

            // Act
            var results = _service.LoadSettings();

            // Assert
            results.Profiles.Should().HaveCount(1);
            results.Profiles[0].DisplayName.Should().Be("My Profile");
        }

        [Fact]
        public void SaveSettings_ShouldWriteToFile()
        {
            // Arrange
            var settings = new AppSettings
            {
                Profiles = new List<ProfileInfo>
                {
                    new ProfileInfo { Id = "Default", DisplayName = "Saved Profile" }
                }
            };
            string? capturedJson = null;
            _fileSystemMock.Setup(f => f.WriteAllText(_settingsPath, It.IsAny<string>()))
                .Callback<string, string>((path, content) => capturedJson = content);

            // Act
            _service.SaveSettings(settings);

            // Assert
            capturedJson.Should().NotBeNull();
            capturedJson.Should().Contain("Saved Profile");
        }
    }
}
