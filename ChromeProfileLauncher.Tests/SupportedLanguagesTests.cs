using System.Collections;
using System.Globalization;
using System.Linq;
using System.Resources;
using ChromeProfileLauncher.Helpers;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class SupportedLanguagesTests
    {
        private readonly ResourceManager _resourceManager = new(
            "ChromeProfileLauncher.Properties.Resources",
            typeof(LocalizationManager).Assembly);

        [Fact]
        public void Items_ShouldContainAllSupportedLanguages()
        {
            SupportedLanguages.Items.Select(i => i.CultureCode).Should()
                .BeEquivalentTo("en", "ja", "zh-CN", "zh-TW", "ko", "fr", "de", "es");
        }

        [Theory]
        [InlineData("en", "en")]
        [InlineData("ja", "ja")]
        [InlineData("zh-CN", "zh-CN")]
        [InlineData("zh-TW", "zh-TW")]
        [InlineData("ko", "ko")]
        [InlineData("fr", "fr")]
        [InlineData("de", "de")]
        [InlineData("es", "es")]
        public void ResolveSupportedLanguage_ShouldReturnExactMatch(string code, string expected)
        {
            SupportedLanguages.ResolveSupportedLanguage(new CultureInfo(code)).Should().Be(expected);
        }

        [Theory]
        [InlineData("en-US", "en")]
        [InlineData("en-GB", "en")]
        [InlineData("ja-JP", "ja")]
        [InlineData("fr-FR", "fr")]
        [InlineData("fr-CA", "fr")]
        [InlineData("es-MX", "es")]
        [InlineData("de-AT", "de")]
        [InlineData("ko-KR", "ko")]
        public void ResolveSupportedLanguage_ShouldResolveViaParentCulture(string code, string expected)
        {
            SupportedLanguages.ResolveSupportedLanguage(new CultureInfo(code)).Should().Be(expected);
        }

        [Theory]
        [InlineData("zh-Hans", "zh-CN")]
        [InlineData("zh-SG", "zh-CN")]
        [InlineData("zh-Hant", "zh-TW")]
        [InlineData("zh-HK", "zh-TW")]
        [InlineData("zh-MO", "zh-TW")]
        public void ResolveSupportedLanguage_ShouldResolveChineseByScriptOrRegion(string code, string expected)
        {
            SupportedLanguages.ResolveSupportedLanguage(new CultureInfo(code)).Should().Be(expected);
        }

        [Theory]
        [InlineData("pt-BR", "en")]
        [InlineData("it-IT", "en")]
        [InlineData("ru-RU", "en")]
        public void ResolveSupportedLanguage_ShouldFallbackToEnglishForUnsupported(string code, string expected)
        {
            SupportedLanguages.ResolveSupportedLanguage(new CultureInfo(code)).Should().Be(expected);
        }

        [Theory]
        [InlineData("ja-JP", "ja")]
        [InlineData("en-US", "en")]
        [InlineData("fr-FR", "fr")]
        [InlineData("zh-HK", "zh-TW")]
        [InlineData(null, "en")]
        [InlineData("", "en")]
        [InlineData("xx-INVALID", "en")]
        public void ResolveSupportedLanguage_StringOverload_ShouldNormalize(string? code, string expected)
        {
            SupportedLanguages.ResolveSupportedLanguage(code).Should().Be(expected);
        }

        [Fact]
        public void AllSupportedLanguages_ShouldHaveAllResourceKeys()
        {
            var neutral = _resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
            var keys = neutral.Cast<DictionaryEntry>().Select(e => (string)e.Key).ToList();
            keys.Should().NotBeEmpty();

            foreach (var option in SupportedLanguages.Items)
            {
                var culture = CultureInfo.GetCultureInfo(option.CultureCode);
                foreach (var key in keys)
                {
                    var value = _resourceManager.GetString(key, culture);
                    value.Should().NotBeNullOrWhiteSpace($"{option.CultureCode} の {key} が翻訳されていること");
                }
            }
        }

        [Fact]
        public void SatelliteLanguages_ShouldReturnTranslatedValue()
        {
            var english = _resourceManager.GetString("LauncherSettings", CultureInfo.InvariantCulture);

            foreach (var option in SupportedLanguages.Items.Where(i => i.CultureCode != "en"))
            {
                var value = _resourceManager.GetString("LauncherSettings", CultureInfo.GetCultureInfo(option.CultureCode));
                value.Should().NotBe(english, $"{option.CultureCode} がニュートラル（英語）にフォールバックしていないこと");
            }
        }
    }
}
