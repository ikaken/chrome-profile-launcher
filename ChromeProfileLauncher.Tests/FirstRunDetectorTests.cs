using ChromeProfileLauncher.Helpers;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class FirstRunDetectorTests
    {
        [Theory]
        [InlineData("true", true, true)]
        [InlineData("true", false, true)]
        [InlineData(null, false, true)]
        [InlineData(null, true, false)]
        [InlineData("false", true, false)]
        public void IsFirstRun_ShouldReturnExpected(string? env, bool settingsFileExists, bool expected)
        {
            FirstRunDetector.IsFirstRun(env, settingsFileExists).Should().Be(expected);
        }
    }
}
