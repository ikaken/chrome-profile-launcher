using System;
using System.Collections.Generic;
using System.Linq;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;
using ChromeProfileLauncher.ViewModels;
using Moq;
using Velopack;
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
        private readonly Mock<IUpdateService> _updateServiceMock;

        public MainViewModelTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _discoveryServiceMock = new Mock<IProfileDiscoveryService>();
            _launcherServiceMock = new Mock<ILauncherService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _updateServiceMock = new Mock<IUpdateService>();

            // Default setups to avoid null refs
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(new List<ProfileInfo>());
            _updateServiceMock.Setup(u => u.GetCurrentVersion()).Returns("1.0.0");
        }

        [Fact]
        public async System.Threading.Tasks.Task LoadProfiles_ShouldMergeSettingsAndDiscovery()
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
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object);
            await vm.InitializationTask;

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
        public async System.Threading.Tasks.Task LoadProfiles_ShouldFilterInvisibleProfiles()
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
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object);
            await vm.InitializationTask;

            // Assert
            vm.Profiles.Should().BeEmpty();
        }

        [Fact]
        public async System.Threading.Tasks.Task LoadProfiles_ShouldRestoreWindowSettings()
        {
            // Arrange
            var settings = new AppSettings
            {
                WindowTop = 150,
                WindowLeft = 200,
                WindowWidth = 500,
                WindowHeight = 600,
                IsMaximized = true
            };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(settings);

            // Act
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object);
            await vm.InitializationTask;

            // Assert
            vm.WindowTop.Should().Be(150);
            vm.WindowLeft.Should().Be(200);
            vm.WindowWidth.Should().Be(500);
            vm.WindowHeight.Should().Be(600);
            vm.WindowState.Should().Be(System.Windows.WindowState.Maximized);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckForUpdatesAsync_WhenAutoUpdateDisabled_ShouldSkipUpdateCheck()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings { EnableAutoUpdate = false });
            _updateServiceMock.Setup(u => u.CheckForUpdatesAsync()).ReturnsAsync((UpdateInfo?)null);

            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object, System.TimeSpan.Zero, false);
            await vm.InitializationTask;

            // Act
            await vm.CheckForUpdatesAsync();

            // Assert
            _updateServiceMock.Verify(u => u.CheckForUpdatesAsync(), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task CheckForUpdatesAsync_WhenAutoUpdateEnabled_ShouldCallUpdateService()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings { EnableAutoUpdate = true });
            _updateServiceMock.Setup(u => u.CheckForUpdatesAsync()).ReturnsAsync((UpdateInfo?)null);

            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object, System.TimeSpan.Zero, false);
            await vm.InitializationTask;

            // Act
            await vm.CheckForUpdatesAsync();

            // Assert
            _updateServiceMock.Verify(u => u.CheckForUpdatesAsync(), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveWindowSettings_ShouldPersistCurrentState()
        {
            // Arrange
            var vm = new MainViewModel(_fileSystemMock.Object, _discoveryServiceMock.Object, _launcherServiceMock.Object, _settingsServiceMock.Object, _updateServiceMock.Object);
            await vm.InitializationTask;
            vm.WindowTop = 300;
            vm.WindowLeft = 400;
            vm.WindowWidth = 800;
            vm.WindowHeight = 600;
            vm.WindowState = System.Windows.WindowState.Normal;

            AppSettings savedSettings = null;
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(s => savedSettings = s);

            // Act
            vm.SaveWindowSettings();

            // Assert
            savedSettings.Should().NotBeNull();
            savedSettings.WindowTop.Should().Be(300);
            savedSettings.WindowLeft.Should().Be(400);
            savedSettings.WindowWidth.Should().Be(800);
            savedSettings.WindowHeight.Should().Be(600);
            savedSettings.IsMaximized.Should().BeFalse();
        }
    }
}
