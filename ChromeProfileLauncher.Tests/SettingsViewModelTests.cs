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

        public SettingsViewModelTests()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object);

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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object);
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
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object);
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
        public void SaveCommand_ShouldInvokeSettingsService()
        {
            // Arrange
            var initial = new List<ProfileInfo>
            {
                new ProfileInfo { Id = "P1", DisplayName = "Name 1" }
            };
            var vm = new SettingsViewModel(initial, _settingsServiceMock.Object);

            // Act
            vm.SaveCommand.Execute(null);

            // Assert
            _settingsServiceMock.Verify(s => s.SaveSettings(It.Is<AppSettings>(asettings => 
                asettings.Profiles.Count == 1 && asettings.Profiles[0].Id == "P1")), Times.Once);
        }
    }
}
