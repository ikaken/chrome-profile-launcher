# カーソルキーによるプロファイル選択

## 背景 / 目的
ランチャーウィンドウ表示中にマウスを使わずキーボードだけでプロファイルを選択・起動できるようにする。
↑/↓ キーで項目を移動し、Enter キーで選択したプロファイルの Chrome を起動する。

## 変更内容
- `MainWindow.xaml`: `ListBox` に `x:Name="ProfileListBox"` を付与し、`KeyDown` イベントと選択ハイライトスタイルを追加。ウィンドウ表示時に ListBox へ自動フォーカス。
- `MainWindow.xaml.cs`: `ProfileListBox_KeyDown` ハンドラ実装（↑/↓ で選択移動、Enter で `LaunchCommand` 実行、Escape でウィンドウを閉じる）。`ShowAndActivate` 時に ListBox へフォーカスを移動。
- `ChromeProfileLauncher.Tests/MainWindowKeyboardTests.cs`: キーボード操作の単体テストを追加。

## 影響範囲
- `MainWindow.xaml` / `MainWindow.xaml.cs`
- ユーザーへの影響: ランチャーが表示されたとき自動的に最初の項目が選択され、キーボードのみで操作可能になる

## 備考
- `ListBoxItem` のカスタムテンプレートにより既存の選択スタイルが無効化されているため、`IsSelected` トリガーを明示的に追加してボーダーカラーで選択状態を表示する。
- Escape キーでウィンドウを閉じる処理も合わせて実装する（利便性向上）。
