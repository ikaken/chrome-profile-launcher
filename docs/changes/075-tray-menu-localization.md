# タスクトレイメニューの多言語対応

## 背景 / 目的
タスクトレイアイコンの右クリックメニュー（「ランチャを開く」「設定」「終了」）が日本語でハードコードされており、
アプリ本体の言語設定（日本語 / English）に追従していない。Issue #75 の要望に従い、
トレイメニューも既存の多言語化機構（Resources.resx / Resources.en.resx + LocalizationProxy）に統合する。

## 変更内容
- `Properties/Resources.resx` / `Properties/Resources.en.resx` にトレイメニュー用のキーを追加
  - `TrayMenuOpenLauncher`（ランチャを開く / Open Launcher）
  - `TrayMenuSettings`（設定 / Settings）
  - `TrayMenuExit`（終了 / Exit）
- `Helpers/LocalizationProxy.cs` に上記 3 キーのプロパティを追加
- `MainWindow.xaml.cs` の `InitializeTaskbarIcon()` でハードコード文字列を廃止し、
  `LocalizationProxy`（`App.xaml` の `StaticResource Resources`）への Binding で `MenuItem.Header` を設定
  → 言語変更時も `LanguageChanged` → `Refresh()` により自動でメニュー表記が切り替わる
- `ChromeProfileLauncher.Tests` にリソースキー整合テストを追加
  （`ResourceManager.GetString(key, culture)` で neutral=ja / en の双方に 3 キーが存在し非空であることを検証）

## 影響範囲
- `MainWindow.InitializeTaskbarIcon()`（トレイメニュー生成部）
- `Helpers.LocalizationProxy`（プロパティ追加のみ）
- `Properties/Resources.resx`, `Properties/Resources.en.resx`（キー追加のみ）
- ユーザーへの影響: 英語設定時にトレイメニューが英語表記になる。日本語表記は従来どおり。

## 備考
- 既存の `Settings` キーは `⚙ 設定` のようにアイコン文字を含むため、トレイメニュー用に別キー
  （`TrayMenuSettings`）を新設してアイコン無しのラベルとする。
- 設定画面での言語切替に即時追従させるため、Header の直接代入ではなく Binding を採用した
  （`ContextMenu` はウィンドウのビジュアルツリー外だが、`Source` を明示した Binding は動作する）。
