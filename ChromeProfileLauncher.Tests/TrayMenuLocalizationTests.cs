using System.Globalization;
using System.Resources;
using ChromeProfileLauncher.Helpers;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class TrayMenuLocalizationTests
    {
        private readonly ResourceManager _resourceManager = new(
            "ChromeProfileLauncher.Properties.Resources",
            typeof(LocalizationManager).Assembly);

        public static TheoryData<string> TrayMenuKeys => new()
        {
            "TrayMenuOpenLauncher",
            "TrayMenuSettings",
            "TrayMenuExit",
        };

        [Theory]
        [MemberData(nameof(TrayMenuKeys))]
        public void TrayMenuKeys_ShouldHaveJapaneseValue(string key)
        {
            var value = _resourceManager.GetString(key, CultureInfo.GetCultureInfo("ja-JP"));

            value.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(TrayMenuKeys))]
        public void TrayMenuKeys_ShouldHaveEnglishValue(string key)
        {
            var japanese = _resourceManager.GetString(key, CultureInfo.GetCultureInfo("ja-JP"));
            var english = _resourceManager.GetString(key, CultureInfo.GetCultureInfo("en-US"));

            english.Should().NotBeNullOrWhiteSpace();
            english.Should().NotBe(japanese, "英語リソースが日本語（ニュートラル）にフォールバックしていないこと");
        }

        [Fact]
        public void LocalizationProxy_ShouldExposeTrayMenuLabels()
        {
            LocalizationManager.SetLanguage("en");
            var proxy = new LocalizationProxy();

            proxy.TrayMenuOpenLauncher.Should().Be("Open Launcher");
            proxy.TrayMenuSettings.Should().Be("Settings");
            proxy.TrayMenuExit.Should().Be("Exit");

            LocalizationManager.SetLanguage("ja");
            proxy.TrayMenuOpenLauncher.Should().Be("ランチャを開く");
            proxy.TrayMenuSettings.Should().Be("設定");
            proxy.TrayMenuExit.Should().Be("終了");
        }
    }
}
