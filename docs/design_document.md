# Chromeプロファイルランチャ 詳細設計書

## 1. システム構成図

本アプリは WPF (.NET 8/9 以降) を使用し、MVVM パターンに基づき設計する。

```text
[View (WPF)] <-> [ViewModel] <-> [Service / Repository] <-> [External Resources]
                                                            (Chrome User Data, 
                                                             Process / Window, 
                                                             Local Settings)
```

## 2. データモデル設計

### 2.1 ProfileInfo (プロファイル情報)
各 Chrome プロファイルを管理するオブジェクト。

| プロパティ名 | 型 | 説明 |
| :--- | :--- | :--- |
| `Id` | `string` | プロファイルディレクトリ名 (例: `Default`, `Profile 1`) |
| `DisplayName` | `string` | ユーザーによる表示名。デフォルトは Chrome で設定された名前。 |
| `IsVisible` | `bool` | ランチャ画面に表示するかどうか。 |
| `IconPath` | `string` | アイコンファイルのパス（PNGまたはICO）。 |
| `IsRunning` | `bool` | 現在起動中かどうか (ViewModel レベルで管理)。 |
| `Hwnd` | `long` | 起動中ウィンドウのハンドル (プロセス監視用)。シリアライズ回避のため long 型。 |

### 2.2 AppSettings (アプリ設定)
`%AppData%\ChromeProfileLauncher\settings.json` に保存される設定。

| プロパティ名 | 型 | 説明 |
| :--- | :--- | :--- |
| `Profiles` | `List<ProfileInfo>` | 管理対象のプロファイル一覧。 |
| `ChromeExePath` | `string` | Chrome の実行ファイルパス。 |

## 3. サービス設計

### 3.1 IProfileDiscoveryService (プロファイル探索)
- `GetAvailableProfiles()`: Chrome の `User Data` フォルダを走査し、`Local State` からプロファイル名とディレクトリ一覧を取得する。
- `GetProfileNameFromLocalState(string profileId)`: JSON 解析によりプロファイル名を抽出。

### 3.2 IIconService (アイコン生成・管理)
- `GetIconPath(string profileId)`: 
    1. プロファイルフォルダ内の `Google Profile Picture.png` を優先的に探す。
    2. なければ `Google Profile.ico` を確認し、あればパスを返す。
    3. いずれもなければ空文字列を返す。`chrome.exe` のアイコンは使用しない。

### 3.3 ILauncherService (起動・フォーカス制御)
- `LaunchOrFocus(ProfileInfo profile)`:
    - `profile.Id` に紐づくウィンドウが既に存在すれば `SetForegroundWindow` でフォーカス。
    - 存在しなければ `--profile-directory` 引数付きで Chrome を起動。
- `MonitorProcess(ProfileInfo profile)`: 
    - 起動したプロセスのウィンドウハンドルを特定し、`ProfileInfo.Hwnd` を更新。
    - ウィンドウが閉じられたことを検知して `IsRunning` を更新。

## 4. UI設計 (WPF)

### 4.1 MainWindow (ランチャ画面)
- **スタイル**: ダークテーマ (#0F0F0F) を基調としたモダンなデザイン。カード（アイテム）背景には視認性向上のため `#222222` を採用。
- **構成**:
    - `ListBox`: 各アイテムを角丸カード形式で表示。`ScrollViewer.CanContentScroll="False"` によるピクセル単位のスムーズスクロールに対応。
    - 各アイテム: アイコン、表示名を**左揃え**で配置。文字色は白 (`#FFFFFF`)。ホバー・クリック時の視覚効果あり。
    - タイトル表示の廃止: レイアウトのコンパクト化のため、「Chrome Launcher」等のタイトル表示を削除。
    - 下部: 「Settings」ボタン（丸みのあるモダンなデザイン）。
    - **DimmerOverlay**: 設定画面表示中にメイン画面全体を半透明（黒）で覆い、グレーアウト状態を表現。

### 4.2 SettingsWindow (設定画面)
- **スタイル**: メイン画面と統一したダークテーマのカードレイアウト。
- **構成**:
    - 各アイテム左側にドラッグ用のハンドル (`☰`) を配置。ドラッグ中はドロップ先のアイテムを半透明 (Opacity 0.4) にすることでドロップ位置を分かりやすく表示。
    - 各プロファイルに表示/非表示を切り替えるトグルスイッチ (CheckBoxスタイル) を配置。
    - プロファイル名の下にフォルダ名 (`Id`) を併記。
    - 各アイテムに 「OPEN (📂)」 ボタンを配置し、直接フォルダを開く機能を提供。
    - 不要なラベル（「NAVIGATION MODULES」等）を削除したシンプルなレイアウト。
    - 保存・キャンセルボタン（イメージ画像に基づく青と黒のボタン）。

## 5. 処理フロー

### 5.1 起動時処理
1. `AppSettings` を読み込む。
2. `IProfileDiscoveryService` で Chrome フォルダを走査し、`AppSettings` にない新規プロファイルがあれば追加。
3. `IIconService` で全プロファイルのアイコンを準備。
4. ウィンドウハンドル監視を開始。
5. メイン画面を表示。

### 5.2 起動・フォーカス処理
1. ボタンクリック時、`ILauncherService` を呼び出し。
2. 既にハンドルを保持しており、そのウィンドウが有効ならフォーカス。
3. 無効ならプロセスを起動。
4. 起動後、しばらくループして新しいウィンドウを特定し、ハンドルを記録。

## 6. 技術スタック・ライブラリ

- **フレームワーク**: .NET 10.0 (WPF)
- **UI フレームワーク**: WPF
- **JSON 解析**: `System.Text.Json`
- **Win32 API**: `P/Invoke` (user32.dll: `SetForegroundWindow`, `ShowWindow`, `IsWindow` など)
- **プロセス監視**: `System.Management` (WMI)

## 7. 配布・ビルド設計 (Deployment)

本アプリは、仕様書の「配布容易なアプリ」を実現するため、以下のビルド構成を採用する。

### 7.1 単一EXE形式 (Self-contained Single-file)
- **方式**: `.NET Runtime` を含めた単一の実行ファイル (`.exe`) として出力。
- **メリット**: ターゲットPCに .NET 10 がインストールされていなくても、EXE単体で即座に動作する。
- **ターゲットアーキテクチャ**: `win-x64` (Windows 10/11 64bit)
- **ビルド設定**:
    - `PublishSingleFile`: `true` (単一ファイル化)
    - `SelfContained`: `true` (ランタイム同梱)
    - `PublishReadyToRun`: `true` (起動速度の最適化)

### 7.2 配布ファイル
- `ChromeProfileLauncher.exe` (約100MB)
- インストーラーは使用せず、実行ファイルを任意のフォルダ（デスクトップやドキュメント等）に配置するだけで使用可能とする。
