using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ChromeProfileLauncher.Helpers
{
    /// <summary>
    /// 言語プルダウンの選択肢。表示名は各言語のネイティブ名を使用する。
    /// </summary>
    public class LanguageOption
    {
        public string CultureCode { get; }
        public string DisplayName { get; }

        public LanguageOption(string cultureCode)
        {
            CultureCode = cultureCode;
            DisplayName = new CultureInfo(cultureCode).NativeName;
        }
    }

    /// <summary>
    /// 対応言語の一元管理。言語を追加する場合は Items と
    /// 対応する Resources.{culture}.resx を追加する。
    /// </summary>
    public static class SupportedLanguages
    {
        public const string FallbackCode = "en";

        public static readonly IReadOnlyList<LanguageOption> Items = new ReadOnlyCollection<LanguageOption>(new[]
        {
            new LanguageOption("en"),
            new LanguageOption("ja"),
            new LanguageOption("zh-CN"),
            new LanguageOption("zh-TW"),
            new LanguageOption("ko"),
            new LanguageOption("fr"),
            new LanguageOption("de"),
            new LanguageOption("es"),
        });

        private static readonly HashSet<string> _codes = new(
            Items.Select(i => i.CultureCode), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 指定カルチャを対応言語コードに解決する。
        /// 完全一致 → 親カルチャ一致 → 中国語は繁体/簡体で振り分け → FallbackCode。
        /// </summary>
        public static string ResolveSupportedLanguage(CultureInfo culture)
        {
            var c = culture;
            while (true)
            {
                if (_codes.Contains(c.Name)) return c.Name;
                if (c.Parent == CultureInfo.InvariantCulture || c.Parent.Equals(c)) break;
                c = c.Parent;
            }

            if (culture.TwoLetterISOLanguageName == "zh")
            {
                var name = culture.Name;
                return name.Contains("Hant") || name.EndsWith("-TW") || name.EndsWith("-HK") || name.EndsWith("-MO")
                    ? "zh-TW"
                    : "zh-CN";
            }

            return FallbackCode;
        }

        /// <summary>
        /// 保存済み言語コードを対応言語コードに正規化する
        /// （旧形式 "ja-JP"/"en-US" → "ja"/"en"、未知コード → FallbackCode）。
        /// </summary>
        public static string ResolveSupportedLanguage(string? languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return FallbackCode;
            try
            {
                return ResolveSupportedLanguage(new CultureInfo(languageCode));
            }
            catch (CultureNotFoundException)
            {
                return FallbackCode;
            }
        }
    }
}
