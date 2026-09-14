# Issue #76: インストール時にスタートアップ登録とタスクトレイ格納の選択を設ける

## 背景 / 目的
Velopackのインストーラーは任意項目を選択するウィザードを提供しないため、現在の自動リリース・自動更新基盤を維持しつつ、インストール直後にスタートアップ登録とタスクトレイ常駐の利用有無を選択できるようにする。

## 変更内容
- Velopackの `OnFirstRun` コールバック（`VELOPACK_FIRSTRUN`）でフラグのみを記録し、新規インストール後の初回起動時だけ初期設定画面を表示する。コールバック内ではウィンドウを表示しない。
- 初期設定画面は単一インスタンス制御（Mutex取得）の後に表示し、多重起動時の二重表示を防ぐ。
- 起動処理中は `ShutdownMode` を `OnExplicitShutdown` へ切り替え、初期設定画面を閉じてもアプリが終了しないようにする。MainWindow表示後に `OnLastWindowClose` 相当の従来動作へ戻す。
- 初期設定画面に「スタートアップに登録する」「タスクトレイに常駐する」の独立したチェック項目と確定ボタンを設ける。
- 両項目は利便性を優先してデフォルトONとし、不要な機能だけをユーザーがOFFにできるようにする。
- スタートアップ設定には既存の `IStartupService` を使用し、タスクトレイ設定には既存の `ISettingsService` と `AppSettings.EnableTaskTray` を使用する。
- 選択確定後に通常のメインウィンドウを生成することで、初回起動からタスクトレイ設定を反映する。
- ウィンドウを閉じて確定しなかった場合もデフォルトの両項目ONとして通常起動し、初回起動判定のみを利用するため次回起動時には再表示しない。
- 日本語・英語のリソースを追加する。設定ファイルが存在しない初回起動時はOSのUI言語で表示し、日本語以外のOSでは英語リソースへフォールバックする。設定保存後は従来どおりアプリの言語設定に従う。
- 初期設定の適用ロジックをViewModelへ分離し、スタートアップ登録、設定保存、各選択値をユニットテストする。

## 影響範囲
- `App.xaml.cs`: Velopack初回起動判定と初期設定画面の表示順序。
- `Views/FirstRunSetupWindow.xaml` / `.xaml.cs`: 初回設定UI。
- `ViewModels/FirstRunSetupViewModel.cs`: 選択状態と設定適用処理。
- `Services/StartupService.cs`, `Services/SettingsService.cs`: 既存インターフェースを再利用し、責務は変更しない。
- `Properties/Resources.resx`, `Properties/Resources.en.resx`: 初回設定画面の文言。
- `ChromeProfileLauncher.Tests`: 初回設定適用処理のテスト。
- 新規インストール後の初回起動にのみ画面が1回追加される。アップデート後および通常起動には表示しない。

## 備考
- 本番リリースは `.github/workflows/release.yml` のVelopack経路であり、`installer/setup.iss` は使用されない。Inno Setupへ切り替えず、Velopackによる自動更新との整合性を優先する。
- 初回設定画面はインストーラー内ではなく、Velopackがインストール完了後に起動するアプリ内で表示する。
- 既存の設定画面から、両項目を後から変更できる動作は維持する。
- スタートアップ登録パスは既存の `StartupService`（実行ファイルパス）を踏襲する。Velopackの `current` ディレクトリはアップデート後も同一パスであるため、登録値は維持される。
- `installer/setup.iss` は本番で未使用のため今回の対象外とする。誤解防止のため、未使用である旨のコメントを追記する。
- 開発実行・ポータブル実行では `VELOPACK_FIRSTRUN` が設定されないため、初期設定画面は表示されない（既存動作のまま）。
