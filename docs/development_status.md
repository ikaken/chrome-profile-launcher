# Chromeプロファイルランチャ 開発状況報告書 (2026-05-01)

## 1. プロジェクト概要
Google Chromeの複数プロファイルを効率的に管理・切替・起動するためのWPFアプリケーション。

## 2. 現在の実装状況

### Phase 1: MVP (完了)
- [x] **プロジェクト基盤**: .NET 10.0 WPFを使用したMVVMアーキテクチャの構築。
- [x] **プロファイル自動検出**: Chromeの `Local State` を解析し、存在するプロファイル（Default, Profile Xなど）を自動取得。
- [x] **起動機能**: `--profile-directory` 引数を使用して、指定したプロファイルでChromeを起動。
- [x] **基本UI**: プロファイル一覧を表示し、クリックで起動するメイン画面の実装。

### Phase 2: 機能拡張 (完了)
- [x] **フォーカス制御 (LaunchOrFocus) & 刷新版**:
    - **ウィンドウ差分検出**: 起動前後の `EnumWindows` スナップショット差分による新規ウィンドウ検出を実装済み。
    - **ウィンドウクラス名判定**: `Chrome_WidgetWin_1` + プロセス名チェックによる確実なフィルタリングを実装済み。
    - **LOCKファイル確認**: `SingletonLock` による起動状態の一次判定を統合。
    - **プロセス差分検出**: 起動前後のPID一覧から新規PIDを特定し、`--type`なしのメインプロセスをフィルタリングするロジックを実装済み。
    - **高度なフォーカス**: `AttachThreadInput` によるフォアグラウンド制限回避を実装済み。
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
- [x] **セキュリティソフト対応のアイコン取得 (Issue #4)**:
    - `Google Profile Picture.png` および `Google Profile.ico` の読み取り可能性（CanReadFile）をチェックするロジックを導入。
    - セキュリティソフトによりファイルアクセスが制限されている場合、自動的に `chrome.exe` 本体のアイコンを抽出して表示する多段フォールバックを実装。
    - EXEアイコン抽出用の `ValueConverter` を導入し、UI 側で透過的に表示可能に改善。

### Phase 3: UI/UX ブラッシュアップ (完了)
- [x] **視認性の向上**:
    - **カード背景の調整**: リストアイテムの背景色を `#222222` に変更し、メイン背景に対してカードが際立つように修正。
    - **文字色の明示**: プロファイル名の文字色を白 (`#FFFFFF`) に固定し、ダークテーマ下での可読性を確保。
    - **不要な装飾の排除**: アイコン背面のオレンジサークルを削除し、清潔感のあるモダンな外観へ変更。
    - **アプリアイコンの適用**: Issue #6 対応。提供された画像を変換し、アプリケーション全体のアイコン(pack URI)として設定。
- [x] **レイアウトの最適化 (コンパクト化)**:
    - **タイトル表示の廃止**: メイン画面上部の「Chrome Launcher」ラベルを削除し、垂直方向のスペースを節約。
    - **余白の最小化**: 各カードの余白や間隔を微調整し、一度に多数のプロファイルを一覧しやすく改善。
- [x] **操作感の向上**:
    - **スムーズスクロール**: `ScrollViewer` のスクロール挙動をピクセル単位に変更し、レスポンスを滑らかに。
    - **ドラッグフィードバック**: 設定画面での並び替え中、ドロップ対象となるアイテムを半透明化することで、移動先を分かりやすく表示。
    - **UI の整理**: 設定画面から不要な補助テキスト（「NAVIGATION MODULES」）を削除。

### Phase 4: キーボードナビゲーション & スクロール修正 (完了)
- [x] **カーソルキー選択**: ↑/↓キーでプロファイルを選択移動。
- [x] **Enterキー起動**: 選択中のプロファイルをEnterキーで起動。
- [x] **Escapeキー閉鎖**: Escapeキーでランチャを閉じる。
- [x] **選択ハイライト**: 選択中の `ListBoxItem` に青枠（`AccentBlueBrush`）を表示。
- [x] **フォーカス自動設定**: ウィンドウ表示時に `ProfileListBox` に自動フォーカス。
- [x] **条件付きスクロール**: 選択項目がビューポート外に出る場合のみスクロール。見えている項目への移動ではスクロールしない。
  - **原因**: `ListBox.OnKeyDown` が `PreviewKeyDown` より後に実行され、`NavigateByLine` 経由で `UpdateLayout` が走り `ScrollViewer` をリセットしていた。
  - **解決**: キー処理を `KeyDown` から `PreviewKeyDown` に移動し `e.Handled = true` で `ListBox.OnKeyDown` をブロック。
  - **スクロール実装**: `FindParent<ScrollViewer>` でアイテムコンテナから正しい `ScrollViewer` を取得し、`TranslatePoint` によるピクセル座標計算で `ScrollToVerticalOffset` を呼び出す。

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

## 5. テスト・ビルド状況 (2026-05-01)
- [x] **自動テスト**: 合計 14 件すべてのテストをパス。
- [x] **デバッグビルド**: 成功。生成パス: `bin\Debug\net10.0-windows\win-x64\ChromeProfileLauncher.exe`
- [x] **不具合修正**:
    - **Issue #19 設定保存時の位置リセット**: 設定保存後にウィンドウ位置が初期化される問題を修正。
    - **Issue #13 設定画面が開かない**: サブフォルダ内の XAML からリソースパス（アイコン）が正しく解決できない問題を修正。
    - **Issue #3 Gmailアイコンの表示**: `Google Profile Picture.png` の権限およびパス解決ロジックを改善。
    - **Issue #24 スタートアップへの登録・解除**: インストーラーによる初回設定と設定画面での動的切り替えを実現。

## 6. 次のステップ (GitHub Issue同期)
1. **Issue #18 タスクトレイ常駐化**: 最小化時にトレイに格納し、バックグラウンド監視を継続する。
2. **Issue #16 自動アップデート対応**: オンラインでのバージョンチェックと更新通知機能。
3. **Issue #10 広告表示対応**: 設定画面等へのスポンサーリンク配置検討。
4. **Issue #8 ポータブル版対応**: 相対パスを利用したUSBメモリ等での運用サポート。
