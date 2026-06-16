# 変更履歴: Issue #55 ドネーションの多言語対応

## 概要
設定画面にあるドネーション（開発支援）セクションのテキストを多言語（日本語・英語）に対応させます。

## 変更内容
- `Properties/Resources.resx`, `Properties/Resources.en.resx`: 以下のリソースキーを追加。
    - `DonationTitle`: 「☕ 開発を支援」 / "☕ Support Development"
    - `DonationDescription`: 支援のお願い文
    - `DonationGitHub`: 「💖 GitHub Sponsors で支援 (月額・単発)」 / "💖 Support on GitHub Sponsors (Monthly/One-time)"
    - `DonationKofi`: 「☕ Ko-fi で応援 (単発・少額)」 / "☕ Support on Ko-fi (One-time/Small)"
- `Helpers/LocalizationProxy.cs`: 上記リソースを参照するためのプロパティを追加。
- `Views/SettingsWindow.xaml`: ドネーションセクションのハードコードされたテキストをバインディングに置き換え。

## 影響範囲
- 設定画面（アプリ設定タブ）のドネーションセクション。

## 検証項目
- [ ] 設定画面を開き、ドネーションセクションのテキストが正しく表示されていること。
- [ ] 言語を英語に切り替えた際、ドネーションセクションのテキストが英語に更新されること。
