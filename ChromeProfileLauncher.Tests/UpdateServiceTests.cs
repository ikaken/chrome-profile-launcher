using System.Threading.Tasks;
using ChromeProfileLauncher.Services;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class UpdateServiceTests
    {
        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenPackaged()
        {
            var service = new UpdateService(isPackaged: true);

            var result = await service.CheckForUpdatesAsync();

            result.Should().BeNull();
        }

        [Fact]
        public void IsUpdateSupported_ShouldBeFalse_WhenPackaged()
        {
            var service = new UpdateService(isPackaged: true);

            service.IsUpdateSupported.Should().BeFalse();
        }

        [Fact]
        public void IsUpdateSupported_ShouldBeTrue_WhenNotPackaged()
        {
            var service = new UpdateService(isPackaged: false);

            service.IsUpdateSupported.Should().BeTrue();
        }

        [Fact]
        public void GetCurrentVersion_ShouldReturnNonEmptyVersion()
        {
            var service = new UpdateService(isPackaged: true);

            var version = service.GetCurrentVersion();

            version.Should().NotBeNullOrEmpty();
            version.Should().NotBe("0.0.0");
        }
    }
}
