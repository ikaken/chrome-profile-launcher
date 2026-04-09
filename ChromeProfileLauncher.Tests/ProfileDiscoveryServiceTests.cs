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
    public class ProfileDiscoveryServiceTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IIconService> _iconServiceMock;
        private readonly ProfileDiscoveryService _service;

        public ProfileDiscoveryServiceTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _iconServiceMock = new Mock<IIconService>();
            _service = new ProfileDiscoveryService(_iconServiceMock.Object, _fileSystemMock.Object);
        }

        [Fact]
        public void GetAvailableProfiles_ShouldReturnProfiles_WhenLocalStateExists()
        {
            // Arrange
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var localStatePath = System.IO.Path.Combine(localAppData, "Google", "Chrome", "User Data", "Local State");

            var json = @"
            {
                ""profile"": {
                    ""info_cache"": {
                        ""Default"": { ""name"": ""Person 1"" },
                        ""Profile 1"": { ""name"": ""Work"" }
                    }
                }
            }";

            _fileSystemMock.Setup(f => f.FileExists(localStatePath)).Returns(true);
            _fileSystemMock.Setup(f => f.ReadAllText(localStatePath)).Returns(json);
            _iconServiceMock.Setup(i => i.GetIconPath(It.IsAny<string>())).Returns("mock/path.png");

            // Act
            var results = _service.GetAvailableProfiles().ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.Id == "Default" && p.DisplayName == "Person 1");
            results.Should().Contain(p => p.Id == "Profile 1" && p.DisplayName == "Work");
        }

        [Fact]
        public void GetAvailableProfiles_ShouldReturnEmpty_WhenLocalStateDoesNotExist()
        {
            // Arrange
            _fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            var results = _service.GetAvailableProfiles();

            // Assert
            results.Should().BeEmpty();
        }
    }
}
