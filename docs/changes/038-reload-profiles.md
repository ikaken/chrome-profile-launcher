# 変更履歴: Issue #38 プロファイル追加時のリロード機能

## 概要
Chromeに新しいプロファイルが追加された際、アプリを再起動せずに設定画面からプロファイル一覧を更新できる機能を追加します。

## 変更内容
- `Resources.resx`, `Resources.en.resx`: 「プロファイルのリロード」用の文字列を追加。
- `Helpers/LocalizationProxy.cs`: XAMLからリロード用の文字列を参照できるようプロパティを追加。
- `ViewModels/SettingsViewModel.cs`:
    *   `IProfileDiscoveryService` を依存関係に追加。
    *   `ReloadProfilesCommand` を実装。既存の設定（表示名、表示/非表示、並び順）を維持しつつ、新しいプロファイルを検出し、削除されたプロファイルを除去するロジックを実装。
- `ViewModels/MainViewModel.cs`: `SettingsViewModel` 生成時に `IProfileDiscoveryService` を渡すよう修正。
- `Views/SettingsWindow.xaml`: プロファイル管理タブに「リロード」ボタンを追加。

## 影響範囲
- 設定画面のプロファイル一覧操作。

## 検証項目
- [ ] 設定画面を開き、「リロード」ボタンが表示されていること。
- [ ] Chrome側でプロファイルを追加・削除した後に「リロード」ボタンを押し、一覧が正しく更新されること。
- [ ] リロードしても既存プロファイルの並び順や表示設定が維持されていること。
