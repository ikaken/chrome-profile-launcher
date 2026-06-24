# 任意のホットキーの設定

## 背景 / 目的

現在、ランチャーを起動するホットキーは「Altキーのダブルタップ」にハードコードされている。
ユーザーによってはAltキーが使いにくい場合や、他のアプリとの競合が発生する場合があるため、
設定画面から任意のキーに変更できるようにする。デフォルトは従来通り「Altキー」とする。

## 変更内容

- `Services/SettingsService.cs` - `AppSettings` に `HotkeyKey` プロパティ（`string`型、デフォルト `"Alt"`）を追加
- `Services/KeyboardTriggerService.cs` - ハードコードされた `Key.LeftAlt / RightAlt` を `IsTargetKey()` による動的判定に変更。`UpdateHotkeyKey()` メソッドを追加し設定変更を即時反映可能に
- `App.xaml.cs` - 起動時に `settings.HotkeyKey` を `KeyboardTriggerService` に渡して初期化。`UpdateHotkeyKey()` メソッドを公開
- `ViewModels/MainViewModel.cs` - 設定ダイアログ保存後に `app.UpdateHotkeyKey()` を呼び出し、ホットキー変更を即時反映
- `ViewModels/SettingsViewModel.cs` - `HotkeyKey` プロパティを追加し、保存・読み込みに対応
- `Views/SettingsWindow.xaml` - 「アプリ設定」タブにホットキー選択コンボボックスを追加
- `Helpers/LocalizationProxy.cs` - ホットキー設定関連のプロパティを追加
- `Properties/Resources.resx` - ホットキー設定関連の日本語リソース文字列を追加
- `Properties/Resources.en.resx` - ホットキー設定関連の英語リソース文字列を追加
- `ChromeProfileLauncher.Tests/KeyboardTriggerServiceTests.cs` - 設定キーでのダブルタップ動作テストを追加（None/Ctrl/Shift/左右キー/UpdateHotkeyKey）
- `ChromeProfileLauncher.Tests/SettingsViewModelTests.cs` - `HotkeyKey` の保存・読み込みテストを追加

## 選択可能なホットキー候補

左右どちらのキーを押しても同じキーとして扱う。

| 表示名（日本語） | 表示名（英語） | 保存値（HotkeyKey） | 判定対象の Key |
|---|---|---|---|
| ホットキーなし | No Hotkey | `None` | なし（無効化） |
| Alt キー（デフォルト） | Alt Key (Default) | `Alt` | `LeftAlt`, `RightAlt` |
| Ctrl キー | Ctrl Key | `Ctrl` | `LeftCtrl`, `RightCtrl` |
| Shift キー | Shift Key | `Shift` | `LeftShift`, `RightShift` |

## 影響範囲

- `KeyboardTriggerService` - トリガーキーの判定ロジックを外部から動的に変更可能に
- `App` - `KeyboardTriggerService` の参照を保持し、設定変更を橋渡し
- `MainViewModel` - 設定保存後に `App.UpdateHotkeyKey()` を呼び出す
- `SettingsViewModel` / `SettingsWindow` - ホットキー選択UIを追加（既存設定項目に影響なし）
- `AppSettings` - 新規フィールド追加のみ（後方互換あり：JSONに存在しない場合は `"Alt"` にフォールバック）

## 備考

- `HotkeyKey` は `string` 型で保存し、`KeyboardTriggerService.IsTargetKey()` 内で判定する
- 既存ユーザーの設定ファイルに `HotkeyKey` がない場合、`null` → `"Alt"` にフォールバックして従来動作を維持する
- 設定変更はアプリ再起動不要で即時反映される
- `LocalizationProxy` に新規プロパティを追加し忘れるとUIに文字列が表示されないため、`resx` と `LocalizationProxy` は必ずセットで更新すること
