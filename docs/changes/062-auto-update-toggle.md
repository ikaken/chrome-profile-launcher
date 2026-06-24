# 自動アップデートを off に出来る機能

## 背景 / 目的
特定のバージョンを使い続けたいユーザーがいるため、設定で自動アップデートを無効にできるようにする。手動での「アップデートを確認」ボタンは常に利用可能にする。

## 変更内容
- `AppSettings` に `EnableAutoUpdate` プロパティ（デフォルト `true`）を追加。
- `MainViewModel.CheckForUpdatesAsync` で、起動時の自動アップデートチェックを `EnableAutoUpdate=false` の場合はスキップする。
- `SettingsViewModel` に `EnableAutoUpdate` プロパティを追加し、設定読み込み時に復元・保存時に反映する。
- `SettingsWindow.xaml` の「アプリケーションアップデート」セクションに、自動アップデート ON/OFF トグルを追加する。
- ローカリゼーションリソース（`Resources.resx`, `Resources.en.resx`）に新しいラベル/説明文を追加。
- 単体テストを追加：`MainViewModel` の自動アップデートスキップ、`SettingsViewModel` の保存反映。

## 影響範囲
- `Services/SettingsService.cs`: `AppSettings` 変更。
- `ViewModels/MainViewModel.cs`: 自動アップデートチェックの分岐追加。
- `ViewModels/SettingsViewModel.cs`: プロパティ追加と保存反映。
- `Views/SettingsWindow.xaml`: UI 追加。
- `Properties/Resources.resx`, `Properties/Resources.en.resx`: リソース追加。
- `ChromeProfileLauncher.Tests/`: テスト追加・更新。

## 備考
- 手動アップデート確認ボタンは常に有効。自動チェックのみを抑制する。
- 既存ユーザーは設定ファイルに `EnableAutoUpdate` がないため、デフォルト値 `true` で維持される。
