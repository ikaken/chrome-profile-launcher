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

        public SettingsViewModelTests()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _updateServiceMock = new Mock<IUpdateService>();
            _updateServiceMock.Setup(u => u.GetCurrentVersion()).Returns("1.0.0");
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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object);

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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object);
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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object, _updateServiceMock.Object);
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
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object);

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
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object);
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
            var vm = new SettingsViewModel(null, _settingsServiceMock.Object, _updateServiceMock.Object);

            // Assert
            vm.Language.Should().Be("ja-JP");
        }
    }
}
