# Issue #5: 多重起動しないでほしい

## 背景
現在、アプリを複数起動することが可能であり、ユーザーが誤って複数のウィンドウを開いてしまう可能性がある。

## 目的
アプリを単一インスタンス（Single Instance）に制限し、二重起動を防止する。
また、二重起動が試みられた際には、既に起動しているウィンドウを最前面に表示することが望ましい。

## 変更内容
- [x] Mutex を使用した二重起動チェックの実装
- [x] 既に起動している場合に既存のウィンドウをアクティブにする処理（Win32 API: SetForegroundWindow 等）の実装
- [x] App.xaml.cs の OnStartup イベントでの制御
- [x] App.xaml から StartupUri を削除し、手動起動に変更

## 実装詳細
- `System.Threading.Mutex` を使用して、アプリケーション全体で一意な名前 (`ChromeProfileLauncher-SingleInstance-Mutex`) でロックを取得。
- 二重起動検知時、`NativeMethods.FindWindow` で既存ウィンドウハンドルを取得。
- 既存ウィンドウが最小化状態（`IsIconic`）であれば、`ShowWindow(SW_RESTORE)` で復元。
- `SetForegroundWindow` で既存ウィンドウを最前面へ。
- `App.xaml` の `StartupUri` を廃止し、`OnStartup` で `MainWindow` を `new` して `Show()` する方式に変更。

## 非対応
- 特になし

## 備考
- WPFでの一般的な単一インスタンス実装パターンに従う。
