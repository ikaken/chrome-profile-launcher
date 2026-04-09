using System;
using System.Collections.Generic;
using System.Linq;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;
using ChromeProfileLauncher.ViewModels;
using Moq;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class MainViewModelTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IProfileDiscoveryService> _discoveryServiceMock;
        private readonly Mock<ILauncherService> _launcherServiceMock;
        private readonly Mock<ISettingsService> _settingsServiceMock;

        public MainViewModelTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _discoveryServiceMock = new Mock<IProfileDiscoveryService>();
            _launcherServiceMock = new Mock<ILauncherService>();
            _settingsServiceMock = new Mock<ISettingsService>();

            // Default setups to avoid null refs
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(new List<ProfileInfo>());
        }

        [Fact]
        public void LoadProfiles_ShouldMergeSettingsAndDiscovery()
        {
            // Arrange
            var settingsProfiles = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "Default", DisplayName = "Custom Name", Order = 0 }
            };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings { Profiles = settingsProfiles });

            var discoveredProfiles = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "Default", DisplayName = "Person 1", IconPath = "path/1" },
                new ProfileInfo { Id = "Profile 1", DisplayName = "Person 2", IconPath = "path/2" }
            };
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(discoveredProfiles);

            // Act
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object);

            // Assert
            vm.Profiles.Should().HaveCount(2);
            
            // Should preserve custom name from settings for the matched profile
            var p1 = vm.Profiles.First(p => p.Id == "Default");
            p1.DisplayName.Should().Be("Custom Name");
            p1.IconPath.Should().Be("path/1");

            // Should add the new discovered profile
            var p2 = vm.Profiles.First(p => p.Id == "Profile 1");
            p2.DisplayName.Should().Be("Person 2");
            
            // Should save the merged settings
            _settingsServiceMock.Verify(s => s.SaveSettings(It.IsAny<AppSettings>()), Times.Once);
        }

        [Fact]
        public void LoadProfiles_ShouldFilterInvisibleProfiles()
        {
            // Arrange
            var settingsProfiles = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "Default", IsVisible = false }
            };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings { Profiles = settingsProfiles });

            var discoveredProfiles = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "Default", DisplayName = "Person 1" }
            };
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(discoveredProfiles);

            // Act
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object);

            // Assert
            vm.Profiles.Should().BeEmpty();
        }

        [Fact]
        public void LaunchCommand_ShouldInvokeLauncherService()
        {
            // Arrange
            var profile = new ProfileInfo { Id = "Default", DisplayName = "Test" };
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(new List<ProfileInfo> { profile });
            
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object);

            // Act
            vm.LaunchCommand.Execute(profile);

            // Assert
            _launcherServiceMock.Verify(l => l.LaunchOrFocus(profile), Times.Once);
        }
    }
}
