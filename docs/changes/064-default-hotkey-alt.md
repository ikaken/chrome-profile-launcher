# ホットキーのデフォルトをAltキー2回押しにする

## 背景 / 目的
Ctrlキー2回押しは他のアプリと被るため、デフォルトのホットキーをAltキー2回押しに変更する。

## 変更内容
- `KeyboardTriggerService` のトリガーキーを `Key.LeftCtrl/RightCtrl` から `Key.LeftAlt/RightAlt` に変更。
- イベント名 `CtrlDoubleTapped` → `HotkeyDoubleTapped` にリネーム（汎用化）。
- フィールド名 `_ctrlHeld` → `_keyHeld` にリネーム。
- `App.xaml.cs` のイベントサブスクライブを変更に追従。
- テストケースのキーを `LeftAlt/RightAlt` に更新。

## 影響範囲
- `Services/KeyboardTriggerService.cs`: トリガーキー変更、イベント名・フィールド名リネーム。
- `App.xaml.cs`: イベント名変更に追従。
- `ChromeProfileLauncher.Tests/KeyboardTriggerServiceTests.cs`: テストキー変更。

## 備考
- 任意のホットキー設定機能は Issue #58 で対応予定。本対応ではデフォルト値の変更のみ。
- DoubleClickTime（300ms）の閾値は変更しない。
