# アプリ起動速度アップ

## 背景 / 目的
Issue #49 の対応。アプリの起動時間を短縮する。

## 変更内容
- `MainViewModel` のプロファイル読み込みを非同期化し、Chrome プロファイル検出 / アイコン取得 / 設定保存をバックグラウンドスレッドで実行。UI スレッドのブロックを解消し、ウィンドウを先に表示する。
- `MainWindow` のタスクトレイアイコンクリックを、`LeftClickCommand` / `DoubleClickCommand` 経由から `TrayLeftMouseUp` / `TrayLeftMouseDoubleClick` 直接イベントへ変更。`ShowAndActivate()` を直接呼び出し、Ctrl 2 回押しと同等の応答速度を実現。
- 非同期初期化完了を待てるように `MainViewModel.InitializationTask` を公開。既存の `LoadProfiles()` 同期メソッドは設定ダイアログ用に維持。
- テストを非同期化し、`InitializationTask` の完了を待ってアサーションするように更新。

## 影響範囲
- `MainWindow.xaml.cs`: タスクトレイアイコンのクリック / コンテキストメニュー「ランチャを開く」で直接 `ShowAndActivate()` を呼び出し。
- `ViewModels/MainViewModel.cs`: `LoadProfilesAsync()` 追加、`InitializationTask` 公開、コンストラクタで非同期読み込みを開始。
- `ChromeProfileLauncher.Tests/MainViewModelTests.cs`: 非同期テスト化、`InitializationTask` 待機。

## 備考
- プロファイル数が多い場合でも、ウィンドウが先に表示されることを優先する。
- 既存の動作（言語設定、ウィンドウ位置復元、タスクトレイ常駐、設定ダイアログ）は維持する。
- 起動直後はプロファイル一覧が空の状態でウィンドウが表示され、読み込み完了後に追記される。
