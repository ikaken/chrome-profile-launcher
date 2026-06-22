# Chrome Profile Launcher (WPF Native)

[English](#english) | [日本語](#japanese)

---

<a name="english"></a>
## English

A Windows desktop application for efficiently managing, launching, and switching between multiple Google Chrome profiles.
Launch your target profile with a single click, or instantly focus an already open window.

### 🚀 Key Features

- **Automatic Profile Detection**: Automatically analyzes Chrome's `User Data` and lists all created profiles.
- **Launch & Focus Control**:
    - **If the profile is not running**: Launches Chrome with the specified profile.
    - **If the profile is already running**: Identifies the window and brings it to the foreground (focus/activate).
- **Profile Customization**:
    - **Rename**: Set custom display names for each profile.
    - **Intuitive Reordering**: Change the list order easily using drag handles (☰).
    - **Visibility Settings**: Hide profiles you don't use frequently.
    - **System Tray Support**: Option to minimize to the system tray when closing the window (default is OFF).
    - **Open Profile Folder**: Directly open the profile folder in File Explorer from the settings screen.
- **Multilingual Support**: Supports both English and Japanese (auto-detection and manual selection).
- **Modern UI**: A simple, easy-to-use card-based design with a dark theme.

### ❤️ Support / Sponsor this Project

If this app helps streamline your daily work or browsing, your support is greatly appreciated!

- **GitHub Sponsors** — Monthly support and one-time support are both available.
- **Ko-fi** — Support with a one-time contribution from a cup of coffee.

[![Sponsor this project](https://img.shields.io/badge/Sponsor%20this%20project-%231EAEDB?logo=github&logoColor=white&style=for-the-badge)](https://github.com/sponsors/ikaken)

[Ko-fi で支援する ☕](https://ko-fi.com/ikaken)

---

### 📦 Download

You can download the latest installer from the link below.

**[Download Latest Version (ChromeProfileLauncher-win-Setup.exe)](https://github.com/ikaken/chrome-profile-launcher/releases/download/v0.1.15-beta/ChromeProfileLauncher-win-Setup.exe)**

*Note: Automatically points to the latest release (v0.1.15-beta).*

#### Installation
1. Download and run `ChromeProfileLauncher-win-Setup.exe`.
2. Following the installer will enable the auto-update feature.
3. If already installed, updates are automatically detected on app startup.

### If Windows shows a warning

Since this app is currently not code-signed, Windows SmartScreen may show a warning.

To run:
1. Click "More info"
2. Click "Run anyway"

Or:
1. Right-click the file
2. Properties
3. Check "Unblock"

### 🛠 For Developers

#### Environment
- OS: Windows 10 / 11 (64-bit)
- Dev: .NET 10.0 SDK, Visual Studio 2022 or later

#### Build and Packaging
This app supports Velopack for distribution and auto-updates.

1. **Build Project**
```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=true
```

2. **Install Velopack Tool** (First time only)
```bash
dotnet tool install -g vpk
```

3. **Create Installer**
```bash
vpk pack --packId ChromeProfileLauncher --packVersion 1.0.0 --packDir ./bin/Release/net10.0-windows/win-x64/publish --outputDir ./releases --icon ./Assets/setup-icon.ico
```

### 📂 Project Structure

- `Models/`: Data definitions
- `ViewModels/`: Application logic (MVVM)
- `Views/`: UI definitions (WPF)
- `Services/`: Core services (Profile discovery, launch control)
- `Helpers/`: Utilities (Win32 API, etc.)

### 📄 Documentation

See the `docs/` folder for detailed specifications.

### 📝 License

This project is licensed under the [MIT License](LICENSE).

---

<a name="japanese"></a>
## 日本語

Google Chromeの複数プロファイルを効率的に管理・起動・切替するためのWindowsデスクトップアプリケーションです。
ワンクリックで対象のプロファイルを起動、または既に開いているウィンドウへ瞬時にフォーカスできます。

### 🚀 主な機能

- **プロファイル自動検出**: Chromeの `User Data` を自動解析し、作成済みのプロファイルを一覧表示します。
- **起動・フォーカス制御**:
    - **対象プロファイルが未起動の場合**: 指定したプロファイルでChromeを新規起動します。
    - **対象プロファイルが起動済みの場合**: そのウィンドウを特定し、最前面にフォーカス（アクティブ化）します。
- **プロファイルカスタマイズ**:
    - **表示名の変更**: 自由に名称を設定可能。
    - **直感的な並べ替え**: ドラッグハンドル（☰）を使用してリストの順序を自在に変更。
    - **表示/非表示設定**: 頻繁に使わないプロファイルをリストから隠せます。
    - **トレイアイコンの常駐設定**: ウィンドウを閉じた際にタスクトレイへ常駐させるか選択可能です（デフォルトは常駐OFF）。
    - **プロファイルフォルダを開く**: 設定画面から直接エクスプローラーでプロファイルフォルダを開けます。
- **多言語対応**: 日本語と英語に対応。OSの言語設定の自動反映および手動切り替えが可能です。
- **モダンなUI**: ダークテーマを基調とした、シンプルで使いやすいカード形式のデザイン。

### ❤️ このアプリを応援する

このアプリが便利だと思ったら、開発の継続にご協力いただけるととても嬉しいです！

- **GitHub Sponsors** — 月額支援も一回払いも可能です
- **Ko-fi** — コーヒー1杯分から気軽に応援できます

[![Sponsor this project](https://img.shields.io/badge/Sponsor%20this%20project-%231EAEDB?logo=github&logoColor=white&style=for-the-badge)](https://github.com/sponsors/ikaken)

[Ko-fi で支援する ☕](https://ko-fi.com/ikaken)

---

### 📦 ダウンロード

最新バージョンのインストール用セットアップファイルは、以下のリンクから直接ダウンロードできます。

**[最新版のダウンロードはこちら (ChromeProfileLauncher-win-Setup.exe)](https://github.com/ikaken/chrome-profile-launcher/releases/download/v0.1.15-beta/ChromeProfileLauncher-win-Setup.exe)**

*※v0.1.15-betaへのリリース対応済み。ダウンロードリンクは自動的に最新のリリースを指します。*

#### インストール方法
1. 上記のリンクをクリックして `ChromeProfileLauncher-win-Setup.exe` をダウンロードし、実行してください。
2. インストーラーに従ってインストールすると、自動アップデート機能が有効になります。
3. すでにインストール済みの場合は、アプリ起動時に自動的にアップデートが検知されます。

### Windowsで警告が表示される場合

このアプリは現在コード署名されていないため、WindowsのSmartScreenにより警告が表示されることがあります。

実行するには：
1. 「詳細情報」をクリック
2. 「実行」をクリック

または：
1. ファイルを右クリック
2. プロパティ
3. 「ブロックの解除」にチェック

### 🛠 開発者向け情報

#### 動作環境
- OS: Windows 10 / 11 (64bit)
- 開発環境: .NET 10.0 SDK, Visual Studio 2022 以降

#### ビルドとパッケージング手順
本アプリはインストーラーによる配布と自動アップデート機能（Velopack）に対応しています。

1. **プロジェクトのビルド**
```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=true
```

2. **Velopack ツールのインストール** (初回のみ)
```bash
dotnet tool install -g vpk
```

3. **インストーラーの作成**
```bash
vpk pack --packId ChromeProfileLauncher --packVersion 1.0.0 --packDir ./bin/Release/net10.0-windows/win-x64/publish --outputDir ./releases --icon ./Assets/setup-icon.ico
```

### 📂 プロジェクト構造

- `Models/`: データ定義
- `ViewModels/`: アプリケーションロジック (MVVM)
- `Views/`: UI定義 (WPF)
- `Services/`: コアサービス (プロファイル探索、起動制御)
- `Helpers/`: ユーティリティ (Win32 API等)

### 📄 ドキュメント

詳細な仕様については `docs/` フォルダを参照してください。

### 📝 ライセンス

このプロジェクトは [MIT ライセンス](LICENSE) の下で公開されています
