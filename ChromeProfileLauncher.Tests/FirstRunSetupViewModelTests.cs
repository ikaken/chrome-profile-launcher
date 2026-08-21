using ChromeProfileLauncher.Services;
using ChromeProfileLauncher.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChromeProfileLauncher.Tests
{
    public class FirstRunSetupViewModelTests
    {
        private readonly Mock<ISettingsService> _settingsServiceMock = new();
        private readonly Mock<IStartupService> _startupServiceMock = new();

        public FirstRunSetupViewModelTests()
        {
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(new AppSettings());
        }

        [Fact]
        public void Constructor_ShouldDefaultOptionsToOff()
        {
            var vm = new FirstRunSetupViewModel(_settingsServiceMock.Object, _startupServiceMock.Object, "en-US");

            vm.LaunchAtStartup.Should().BeFalse();
            vm.EnableTaskTray.Should().BeFalse();
        }

        [Fact]
        public void SaveCommand_ShouldRegisterStartupAndPersistSettings()
        {
            AppSettings? savedSettings = null;
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(settings => savedSettings = settings);
            var vm = new FirstRunSetupViewModel(_settingsServiceMock.Object, _startupServiceMock.Object, "en-US")
            {
                LaunchAtStartup = true,
                EnableTaskTray = true
            };
            var completed = false;
            vm.Completed += (_, _) => completed = true;

            vm.SaveCommand.Execute(null);

            _startupServiceMock.Verify(s => s.Register(), Times.Once);
            _startupServiceMock.Verify(s => s.Unregister(), Times.Never);
            savedSettings.Should().NotBeNull();
            savedSettings!.EnableTaskTray.Should().BeTrue();
            savedSettings.Language.Should().Be("en-US");
            completed.Should().BeTrue();
        }

        [Fact]
        public void SaveCommand_ShouldUnregisterStartupWhenOptionIsOff()
        {
            var vm = new FirstRunSetupViewModel(_settingsServiceMock.Object, _startupServiceMock.Object, "ja-JP");

            vm.SaveCommand.Execute(null);

            _startupServiceMock.Verify(s => s.Unregister(), Times.Once);
            _startupServiceMock.Verify(s => s.Register(), Times.Never);
            _settingsServiceMock.Verify(s => s.SaveSettings(It.Is<AppSettings>(settings =>
                !settings.EnableTaskTray && settings.Language == "ja-JP")), Times.Once);
        }

        [Fact]
        public void SaveCommand_ShouldPreserveExistingSettings()
        {
            var existingSettings = new AppSettings
            {
                EnableAutoUpdate = false,
                HotkeyKey = "Ctrl",
                Profiles = new() { new() { Id = "Default" } }
            };
            AppSettings? savedSettings = null;
            _settingsServiceMock.Setup(s => s.LoadSettings()).Returns(existingSettings);
            _settingsServiceMock.Setup(s => s.SaveSettings(It.IsAny<AppSettings>()))
                .Callback<AppSettings>(settings => savedSettings = settings);
            var vm = new FirstRunSetupViewModel(_settingsServiceMock.Object, _startupServiceMock.Object, "en-US");

            vm.SaveCommand.Execute(null);

            savedSettings.Should().BeSameAs(existingSettings);
            savedSettings!.EnableAutoUpdate.Should().BeFalse();
            savedSettings.HotkeyKey.Should().Be("Ctrl");
            savedSettings.Profiles.Should().ContainSingle(profile => profile.Id == "Default");
        }

        [Fact]
        public void SaveCommand_ShouldCompleteWhenServicesFail()
        {
            _startupServiceMock.Setup(s => s.Unregister()).Throws(new InvalidOperationException());
            _settingsServiceMock.Setup(s => s.LoadSettings()).Throws(new InvalidOperationException());
            var vm = new FirstRunSetupViewModel(_settingsServiceMock.Object, _startupServiceMock.Object, "en-US");
            var completed = false;
            vm.Completed += (_, _) => completed = true;

            Action act = () => vm.SaveCommand.Execute(null);

            act.Should().NotThrow();
            completed.Should().BeTrue();
        }
    }
}
