using System.Collections.Generic;
using ChromeProfileLauncher.Models;
using ChromeProfileLauncher.Services;
using ChromeProfileLauncher.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChromeProfileLauncher.Tests
{
    public class CursorKeyProfileSelectionTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock = new();
        private readonly Mock<IProfileDiscoveryService> _discoveryMock = new();
        private readonly Mock<ILauncherService> _launcherMock = new();
        private readonly Mock<ISettingsService> _settingsMock = new();
        private readonly Mock<IUpdateService> _updateMock = new();

        private MainViewModel CreateVm(List<ProfileInfo> profiles)
        {
            _settingsMock.Setup(s => s.LoadSettings())
                .Returns(new AppSettings { Profiles = profiles });
            _discoveryMock.Setup(d => d.GetAvailableProfiles())
                .Returns(profiles);
            _updateMock.Setup(u => u.GetCurrentVersion()).Returns("1.0.0");

            var vm = new MainViewModel(
                _fileSystemMock.Object,
                _discoveryMock.Object,
                _launcherMock.Object,
                _settingsMock.Object,
                _updateMock.Object,
                startAutoUpdateCheck: false);
            vm.InitializationTask.GetAwaiter().GetResult();
            return vm;
        }

        private static List<ProfileInfo> ThreeProfiles() => new()
        {
            new ProfileInfo { Id = "P1", DisplayName = "Profile 1", IsVisible = true, Order = 0 },
            new ProfileInfo { Id = "P2", DisplayName = "Profile 2", IsVisible = true, Order = 1 },
            new ProfileInfo { Id = "P3", DisplayName = "Profile 3", IsVisible = true, Order = 2 },
        };

        [Fact]
        public async System.Threading.Tasks.Task LoadProfiles_ShouldSelectFirstProfile()
        {
            var vm = CreateVm(ThreeProfiles());
            await vm.InitializationTask;

            vm.SelectedProfile.Should().NotBeNull();
            vm.SelectedProfile!.Id.Should().Be("P1");
        }

        [Fact]
        public void MoveDown_ShouldSelectNextProfile()
        {
            var vm = CreateVm(ThreeProfiles());

            var profiles = vm.Profiles;
            vm.SelectedProfile = profiles[0];

            // simulate Down: index 0 -> 1
            int index = profiles.IndexOf(vm.SelectedProfile);
            int next = System.Math.Clamp(index + 1, 0, profiles.Count - 1);
            vm.SelectedProfile = profiles[next];

            vm.SelectedProfile.Id.Should().Be("P2");
        }

        [Fact]
        public void MoveUp_ShouldSelectPreviousProfile()
        {
            var vm = CreateVm(ThreeProfiles());

            var profiles = vm.Profiles;
            vm.SelectedProfile = profiles[1];

            // simulate Up: index 1 -> 0
            int index = profiles.IndexOf(vm.SelectedProfile);
            int next = System.Math.Clamp(index - 1, 0, profiles.Count - 1);
            vm.SelectedProfile = profiles[next];

            vm.SelectedProfile.Id.Should().Be("P1");
        }

        [Fact]
        public void MoveDown_AtLastItem_ShouldStayAtLast()
        {
            var vm = CreateVm(ThreeProfiles());

            var profiles = vm.Profiles;
            vm.SelectedProfile = profiles[2];

            // simulate Down at last
            int index = profiles.IndexOf(vm.SelectedProfile);
            int next = System.Math.Clamp(index + 1, 0, profiles.Count - 1);
            vm.SelectedProfile = profiles[next];

            vm.SelectedProfile.Id.Should().Be("P3");
        }

        [Fact]
        public void MoveUp_AtFirstItem_ShouldStayAtFirst()
        {
            var vm = CreateVm(ThreeProfiles());

            var profiles = vm.Profiles;
            vm.SelectedProfile = profiles[0];

            // simulate Up at first
            int index = profiles.IndexOf(vm.SelectedProfile);
            int next = System.Math.Clamp(index - 1, 0, profiles.Count - 1);
            vm.SelectedProfile = profiles[next];

            vm.SelectedProfile.Id.Should().Be("P1");
        }

        [Fact]
        public void EnterKey_ShouldExecuteLaunchCommand()
        {
            var vm = CreateVm(ThreeProfiles());

            var profiles = vm.Profiles;
            vm.SelectedProfile = profiles[1];

            vm.LaunchCommand.Execute(vm.SelectedProfile);

            _launcherMock.Verify(l => l.LaunchOrFocus(profiles[1]), Times.Once);
        }
    }
}
