# Chromeプロファイルランチャ 開発状況報告書 (2026-04-09)

## 1. プロジェクト概要
Google Chromeの複数プロファイルを効率的に管理・切替・起動するためのWPFアプリケーション。

## 2. 現在の実装状況

### Phase 1: MVP (完了)
- [x] **プロジェクト基盤**: .NET 10.0 WPFを使用したMVVMアーキテクチャの構築。
- [x] **プロファイル自動検出**: Chromeの `Local State` を解析し、存在するプロファイル（Default, Profile Xなど）を自動取得。
- [x] **起動機能**: `--profile-directory` 引数を使用して、指定したプロファイルでChromeを起動。
- [x] **基本UI**: プロファイル一覧を表示し、クリックで起動するメイン画面の実装。

### Phase 2: 機能拡張 (完了)
- [x] **フォーカス制御 (LaunchOrFocus)**: 
    - WMI (Windows Management Instrumentation) を使用して実行中のChromeプロセス引数を解析。
    - 指定プロファイルが既に起動している場合、Win32 API (`SetForegroundWindow`) で対象ウィンドウを最前面に表示。
- [x] **設定の永続化**: `%AppData%\ChromeProfileLauncher\settings.json` への設定保存機能。
- [x] **モダンな設定画面の刷新 (NEW)**:
    - イメージ画像に基づくダークテーマおよびモダンスタイルの適用。
    - **ドラッグハンドルによる並び替え**: リストアイテム左側のハンドル（☰）をドラッグして直感的に順序を変更可能。
    - **プロファイルフォルダを開く (OPEN)**: 各アイテムから該当するChromeプロファイルフォルダをエクスプローラで直接開く機能。
    - **トグルスイッチ方式**: 表示/非表示の切り替えにアニメーション付きトグルスイッチを採用。
    - プロファイル名およびフォルダ名（Id）の同時表示（読み取り専用）。
- [x] **ランチャ画面（メイン画面）の刷新 (NEW)**:
    - 設定画面と統一感のあるモダンなダークテーマ・カード形式のデザインへ刷新。
    - **アイコン・名称の左揃え**: 視認性向上のため、レイアウトを左揃えに統一。
    - **グレーアウト機能 (Dimmer)**: 設定画面を開いている間、メイン画面を半透明のレイヤーで覆い、操作対象を明確化。
    - **アイコン取得ロジックの整理**: `Google Profile Picture.png` を優先し、なければ `Google Profile.ico` を表示。いずれもない場合は空白。
- [x] **機能の整理**: ユーザー設定アイコン機能や `chrome.exe` アイコンの利用を排除し、シンプルで予測可能な挙動へ調整。

### Phase 3: UI/UX ブラッシュアップ (完了)
- [x] **視認性の向上**:
    - **カード背景の調整**: リストアイテムの背景色を `#222222` に変更し、メイン背景に対してカードが際立つように修正。
    - **文字色の明示**: プロファイル名の文字色を白 (`#FFFFFF`) に固定し、ダークテーマ下での可読性を確保。
    - **不要な装飾の排除**: アイコン背面のオレンジサークルを削除し、清潔感のあるモダンな外観へ変更。
- [x] **レイアウトの最適化 (コンパクト化)**:
    - **タイトル表示の廃止**: メイン画面上部の「Chrome Launcher」ラベルを削除し、垂直方向のスペースを節約。
    - **余白の最小化**: 各カードの余白や間隔を微調整し、一度に多数のプロファイルを一覧しやすく改善。
- [x] **操作感の向上**:
    - **スムーズスクロール**: `ScrollViewer` のスクロール挙動をピクセル単位に変更し、レスポンスを滑らかに。
    - **ドラッグフィードバック**: 設定画面での並び替え中、ドロップ対象となるアイテムを半透明化することで、移動先を分かりやすく表示。
    - **UI の整理**: 設定画面から不要な補助テキスト（「NAVIGATION MODULES」）を削除。

## 3. 技術スタック
- **Framework**: .NET 10.0 (WPF)
- **Language**: C# 13.0
- **Libraries**: 
    - `System.Text.Json` (設定・プロファイル解析)
    - `System.Management` (WMIによるプロセス監視)
- **Win32 API**: `user32.dll` (ShowWindow, SetForegroundWindow, EnumWindows等)

## 4. プロジェクト構造
```text
ChromeProfileLauncher/
├── Helpers/
│   ├── RelayCommand.cs (コマンド実装)
│   └── Win32Api.cs (ネイティブAPI定義)
├── Models/
│   └── ProfileInfo.cs (データモデル)
├── Services/
│   ├── IconService.cs (アイコン取得)
│   ├── LauncherService.cs (起動・フォーカス制御)
│   ├── ProfileDiscoveryService.cs (プロファイル探索)
│   └── SettingsService.cs (設定保存)
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── SettingsWindow.xaml (.cs)
│   └── MainWindow.xaml (.cs)
└── ChromeProfileLauncher.csproj
```

## 5. テスト・ビルド状況 (2026-04-09)
- [x] **自動テスト**: 合計 14 件すべてのテストをパス。
    - `ProfileDiscoveryService`, `SettingsService`, `MainViewModel`, `SettingsViewModel`, `IconService` の全単体テストをパス。
- [x] **デバッグビルド**: 成功。
    - `dotnet build --configuration Debug` により正常にビルド。
    - 生成パス: `bin\Debug\net10.0-windows\win-x64\ChromeProfileLauncher.exe`
- [x] **不具合修正**:
    - メインプロジェクトのビルドプロセスでテストプロジェクトのファイルが誤って含まれていた問題を解決 (`ChromeProfileLauncher.csproj` の除外設定を強化)。

## 6. 次のステップ (提案)
1. **タスクトレイ常駐化**: 最小化時にトレイに格納し、素早くアクセス可能にする。
2. **多言語対応の検討**: UIテキストの外部リソース化。
3. **エラーハンドリングの強化**: ログ出力の充実と、ユーザーフレンドリーなエラー表示。
