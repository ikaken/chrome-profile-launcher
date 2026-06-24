# 任意のホットキーの設定

## 背景 / 目的

現在、ランチャーを起動するホットキーは「Altキーのダブルタップ」にハードコードされている。
ユーザーによってはAltキーが使いにくい場合や、他のアプリとの競合が発生する場合があるため、
設定画面から任意のキーに変更できるようにする。デフォルトは従来通り「Altキー」とする。

## 変更内容

- `Services/SettingsService.cs` - `AppSettings` に `HotkeyKey` プロパティ（`string`型、デフォルト `"LeftAlt"`）を追加
- `Services/KeyboardTriggerService.cs` - ハードコードされた `Key.LeftAlt / RightAlt` を、設定値から動的に決定するロジックに変更
- `ViewModels/SettingsViewModel.cs` - `HotkeyKey` プロパティを追加し、保存・読み込みに対応
- `Views/SettingsWindow.xaml` - 「アプリ設定」タブにホットキー選択コンボボックスを追加
- `Properties/Resources.resx` - ホットキー設定関連の日本語リソース文字列を追加
- `Properties/Resources.en.resx` - ホットキー設定関連の英語リソース文字列を追加
- `ChromeProfileLauncher.Tests/KeyboardTriggerServiceTests.cs` - 設定キーでのダブルタップ動作テストを追加
- `ChromeProfileLauncher.Tests/SettingsViewModelTests.cs` - `HotkeyKey` の保存・読み込みテストを追加

## 選択可能なホットキー候補

左右どちらのキーを押しても同じキーとして扱う。

| 表示名（日本語） | 表示名（英語） | 保存値（HotkeyKey） | 判定対象の Key |
|---|---|---|---|
| Alt キー（デフォルト） | Alt Key (Default) | `Alt` | `LeftAlt`, `RightAlt` |
| Ctrl キー | Ctrl Key | `Ctrl` | `LeftCtrl`, `RightCtrl` |
| Shift キー | Shift Key | `Shift` | `LeftShift`, `RightShift` |

## 影響範囲

- `KeyboardTriggerService` - トリガーキーの判定ロジックを外部から注入可能に変更
- `SettingsViewModel` / `SettingsWindow` - ホットキー選択UIを追加（既存設定項目に影響なし）
- `AppSettings` - 新規フィールド追加のみ（後方互換あり：JSONに存在しない場合はデフォルト値を使用）

## 備考

- `HotkeyKey` は `string` 型で保存し、`KeyboardTriggerService` 内で `System.Windows.Input.Key` に変換する
- 既存ユーザーの設定ファイルに `HotkeyKey` がない場合でも、`null` → `"LeftAlt"` のフォールバックで従来動作を維持する
- Win キー（`LWin`）はシステム予約キーのため、選択時に注意書きを表示することを検討したが、今回は選択肢として追加するのみとする
