# アプリアイコンの作成

## 背景
Issue #6 (アプリアイコンの作成) 対応。
ユーザーが提供した `Assets/chrome-profile-launcher.png` を元に、アプリケーションのアイコンを設定する。

## 目的
WPFアプリケーションのウィンドウアイコンおよび実行ファイル（.exe）のアイコンとして、提供された画像を使用できるようにする。
また、実行環境からのパス解決の不具合を防ぐために `pack URI` を用いてアイコンを指定する。

## 変更内容
1. `Assets/chrome-profile-launcher.png` を `Assets/app.ico` に変換した。（16x16, 32x32, 48x48, 64x64, 128x128, 256x256 のマルチサイズ構成）
2. `MainWindow.xaml` の `Icon` プロパティを `pack://application:,,,/ChromeProfileLauncher;component/Assets/app.ico` に変更。
3. `Views/SettingsWindow.xaml` の `Icon` プロパティを同様に `pack URI` に変更。
4. `ChromeProfileLauncher.csproj` は既に `Assets\app.ico` を `ApplicationIcon` として指定していたため変更なし。

## 影響範囲
- アプリケーション起動時のウィンドウアイコン
- タスクバーのアイコン
- 実行ファイルのアイコン

## 非対応
特になし。

## 備考
パス解決の安定化のため、サブフォルダからの相対パス(`Assets/app.ico`)指定から、`pack URI` に変更している。
