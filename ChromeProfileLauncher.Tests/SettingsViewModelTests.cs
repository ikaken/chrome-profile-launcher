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
    public class SettingsViewModelTests
    {
        private readonly Mock<ISettingsService> _settingsServiceMock;
        private readonly Mock<IUpdateService> _updateServiceMock;
        private readonly Mock<IProfileDiscoveryService> _discoveryServiceMock;

        public SettingsViewModelTests()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());
            _updateServiceMock = new Mock<IUpdateService>();
            _updateServiceMock.Setup(u => u.GetCurrentVersion()).Returns("1.0.0");
            _discoveryServiceMock = new Mock<IProfileDiscoveryService>();
        }

        [Fact]
        public void Constructor_ShouldInitializeWithProfiles()
        {
            // Arrange
            var initial = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", DisplayName = "Profile 1" },
                new ProfileInfo { Id = "P2", DisplayName = "Profile 2" }
            };

            // Act
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.Profiles.Should().HaveCount(2);
            vm.Profiles[0].Id.Should().Be("P1");
            vm.Profiles[1].Id.Should().Be("P2");
        }

        [Fact]
        public void MoveUpCommand_ShouldReorderProfiles()
        {
            // Arrange
            var initial = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", Order = 0 },
                new ProfileInfo { Id = "P2", Order = 1 }
            };
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);
            var p2 = vm.Profiles[1];

            // Act
            vm.MoveUpCommand.Execute(p2);

            // Assert
            vm.Profiles[0].Id.Should().Be("P2");
            vm.Profiles[0].Order.Should().Be(0);
            vm.Profiles[1].Id.Should().Be("P1");
            vm.Profiles[1].Order.Should().Be(1);
        }

        [Fact]
        public void MoveDownCommand_ShouldReorderProfiles()
        {
            // Arrange
            var initial = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", Order = 0 },
                new ProfileInfo { Id = "P2", Order = 1 }
            };
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);
            var p1 = vm.Profiles[0];

            // Act
            vm.MoveDownCommand.Execute(p1);

            // Assert
            vm.Profiles[0].Id.Should().Be("P2");
            vm.Profiles[0].Order.Should().Be(0);
            vm.Profiles[1].Id.Should().Be("P1");
            vm.Profiles[1].Order.Should().Be(1);
        }

        [Fact]
        public void Constructor_ShouldRestoreSettingsWindowSettings()
        {
            // Arrange
            var settings = new AppSettings
            {
                SettingsWindowTop = 300,
                SettingsWindowLeft = 400,
                SettingsWindowWidth = 800,
                SettingsWindowHeight = 600
            };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(settings);

            // Act
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.WindowTop.Should().Be(300);
            vm.WindowLeft.Should().Be(400);
            vm.WindowWidth.Should().Be(800);
            vm.WindowHeight.Should().Be(600);
        }

        [Fact]
        public void SaveWindowSettings_ShouldPersistCurrentState()
        {
            // Arrange
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);
            vm.WindowTop = 500;
            vm.WindowLeft = 600;
            vm.WindowWidth = 1000;
            vm.WindowHeight = 800;

            AppSettings savedSettings = null;
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(s => savedSettings = s);

            // Act
            vm.SaveWindowSettings();

            // Assert
            savedSettings.Should().NotBeNull();
            savedSettings.SettingsWindowTop.Should().Be(500);
            savedSettings.SettingsWindowLeft.Should().Be(600);
            savedSettings.SettingsWindowWidth.Should().Be(1000);
            savedSettings.SettingsWindowHeight.Should().Be(800);
        }

        [Fact]
        public void Constructor_ShouldInitializeWithDefaultLanguage()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());

            // Act
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.Language.Should().Be("ja-JP");
        }

        [Fact]
        public void Constructor_ShouldRestoreEnableAutoUpdate()
        {
            // Arrange
            var settings = new AppSettings { EnableAutoUpdate = false };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(settings);

            // Act
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.EnableAutoUpdate.Should().BeFalse();
        }

        [Fact]
        public void SaveCommand_ShouldPersistEnableAutoUpdate()
        {
            // Arrange
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);
            vm.EnableAutoUpdate = false;

            AppSettings savedSettings = null;
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(s => savedSettings = s);

            // Act
            vm.SaveCommand.Execute(null);

            // Assert
            savedSettings.Should().NotBeNull();
            savedSettings.EnableAutoUpdate.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ShouldRestoreHotkeyKey()
        {
            // Arrange
            var settings = new AppSettings { HotkeyKey = "Ctrl" };
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(settings);

            // Act
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.HotkeyKey.Should().Be("Ctrl");
        }

        [Fact]
        public void Constructor_ShouldDefaultHotkeyKeyToAlt()
        {
            // Arrange
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());

            // Act
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            // Assert
            vm.HotkeyKey.Should().Be("Alt");
        }

        [Fact]
        public void SaveCommand_ShouldPersistHotkeyKey()
        {
            // Arrange
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);
            vm.HotkeyKey = "Shift";

            AppSettings savedSettings = null;
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(s => savedSettings = s);

            // Act
            vm.SaveCommand.Execute(null);

            // Assert
            savedSettings.Should().NotBeNull();
            savedSettings.HotkeyKey.Should().Be("Shift");
        }

        [Fact]
        public void ReloadProfilesCommand_ShouldMergeDetectedProfiles()
        {
            // Arrange
            var initial = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", DisplayName = "Custom P1", Order = 0 },
                new ProfileInfo { Id = "P2", DisplayName = "P2", Order = 1 }
            };
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object, _discoveryServiceMock.Object);

            var detected = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", DisplayName = "Default P1" }, // P1 exists
                new ProfileInfo { Id = "P3", DisplayName = "New P3" }      // P2 removed, P3 added
            };
            _discoveryServiceMock.Setup(d => d.GetAvailableProfiles()).Returns(detected);

            // Act
            vm.ReloadProfilesCommand.Execute(null);

            // Assert
            vm.Profiles.Should().HaveCount(2);
            vm.Profiles[0].Id.Should().Be("P1");
            vm.Profiles[0].DisplayName.Should().Be("Custom P1"); // Should preserve custom name
            vm.Profiles[1].Id.Should().Be("P3");
            vm.Profiles[1].DisplayName.Should().Be("New P3");
            vm.Profiles.Any(p => p.Id == "P2").Should().BeFalse(); // P2 should be removed
        }
    }
}
