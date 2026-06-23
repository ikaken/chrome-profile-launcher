# ctrlキー長押しでランチャが表示される

## 背景 / 目的
Issue #63 の対応。Ctrlキーのダブルタップでランチャーを表示する機能において、Ctrl長押し（キーリピート）でもランチャーが表示されてしまうバグを修正する。

## 原因
`KeyboardTriggerService` が `KeyDown` イベントのみを監視していたため、Ctrl長押し時のOSキーリピートによる連続 `KeyDown` を2回目のタップと誤判定していた。

## 変更内容
- `KeyboardHookHelper` に `KeyUp` イベントを追加。`WM_KEYUP` / `WM_SYSKEYUP` を検出する。
- `KeyboardTriggerService` のダブルタップ検出ロジックを3ステップ方式に変更:
  1. Ctrl `KeyDown` → タイマー開始（キーリピートの連続KeyDownは無視）
  2. Ctrl `KeyUp` → 1回目のタップ完了を記録
  3. Ctrl `KeyDown` → 300ms以内なら即座にダブルタップ成立
- テスト用コンストラクタ（`noHook`）と `SimulateKeyDown` / `SimulateKeyUp` メソッドを追加し、ロジックの単体テストを可能にした。

## 影響範囲
- `Helpers/KeyboardHookHelper.cs`: `KeyUp` イベント追加、`WM_KEYUP` / `WM_SYSKEYUP` 定数追加。
- `Services/KeyboardTriggerService.cs`: ダブルタップ検出ロジックの修正、テスト用API追加。
- `ChromeProfileLauncher.Tests/KeyboardTriggerServiceTests.cs`: 6テストケースを追加。
- `ChromeProfileLauncher.csproj`: `InternalsVisibleTo` 追加。

## 備考
- 3ステップ方式により、2回目のKeyDown時点で即座にイベント発火するため、レスポンスの低下はゼロ。
- 他のキーが間に入った場合はリセットする（既存動作を維持）。
- DoubleClickTime（300ms）の閾値は変更しない。
