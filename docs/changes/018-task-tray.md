# Issue #18: タスクトレイへの常駐

## 背景 / 目的
アプリを最小化または閉じた際にタスクトレイに常駐させることで、バックグラウンドでの待機を可能にし、必要な時に素早くアクセスできるようにする。

## 変更内容
- `H.NotifyIcon.Wpf` ライブラリを導入し、タスクトレイ常駐機能を実装。
- `SettingsService` に「最小化時にトレイへ格納」「閉じるときにトレイへ格納」の設定（`MinimizeToTray`, `CloseToTray`）を追加。初期値は両方有効（true）。
- `MainWindow.xaml` に `TaskbarIcon` を定義。ダブルクリックでウィンドウ復元、右クリックでコンテキストメニュー（開く、設定、終了）を表示。
- `MainViewModel` に `ShowWindowCommand`（ウィンドウ表示・復元）と `ExitApplicationCommand`（アプリ終了）を追加。
- `MainWindow.xaml.cs` の `Window_Closing` および `Window_StateChanged` イベントでトレイ格納ロジックを実装。
- `App.xaml.cs` にて `Mutex` と `NamedPipe` を使用した二重起動防止と、既存インスタンスのウィンドウ活性化（IPC）を実装。

## 影響範囲
- `Services/SettingsService.cs`, `ViewModels/SettingsViewModel.cs`, `Views/SettingsWindow.xaml`
- `ViewModels/MainViewModel.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`
- `App.xaml.cs`, `ChromeProfileLauncher.csproj`

## 備考
- `H.NotifyIcon.Wpf` を使用することで、MVVM パターンに則ったコマンドバインディングを実現。
- 二重起動時にトレイ格納中の既存インスタンスを復元するため、Named Pipe によるプロセス間通信（IPC）を採用。
