# Chromeプロファイルランチャ 詳細設計書

> **本設計書の位置づけ**: 本書はアプリケーションの最終的な完成形を記述する。現在の実装状況については [development_status.md](./development_status.md) を参照のこと。未実装の機能には **【未実装】** マークを付与している。

## 1. システム構成図

本アプリは WPF (.NET 10.0) を使用し、MVVM パターンに基づき設計する。

```text
[View (WPF)] <-> [ViewModel] <-> [Service / Repository] <-> [External Resources]
                                                            (Chrome User Data, 
                                                             Process / Window, 
                                                             Local Settings)
```

## 2. データモデル設計

### 2.1 ProfileInfo (プロファイル情報)
各 Chrome プロファイルを管理するオブジェクト。

| プロパティ名 | 型 | 永続化 | 説明 |
| :--- | :--- | :--- | :--- |
| `Id` | `string` | ○ | プロファイルディレクトリ名 (例: `Default`, `Profile 1`) |
| `DisplayName` | `string` | ○ | ユーザーによる表示名。デフォルトは Chrome で設定された名前。 |
| `IsVisible` | `bool` | ○ | ランチャ画面に表示するかどうか。 |
| `Order` | `int` | ○ | 表示順序（0始まり）。設定画面での並び替えに使用。 |
| `IconPath` | `string` | ○ | アイコンファイルのパス（PNGまたはICO）。 |
| `IsRunning` | `bool` | × | 現在起動中かどうか (LOCKファイルおよびウィンドウ存在確認に基づく)。ランタイム専用 (`[JsonIgnore]`)。 |
| `Hwnd` | `IntPtr` | × | 起動中ウィンドウのハンドル (一意識別に利用)。ランタイム専用 (`[JsonIgnore]`)。 |

### 2.2 AppSettings (アプリ設定)
`%AppData%\ChromeProfileLauncher\settings.json` に保存される設定。

| プロパティ名 | 型 | 実装状況 | 説明 |
| :--- | :--- | :--- | :--- |
| `Profiles` | `List<ProfileInfo>` | 実装済み | 管理対象のプロファイル一覧。 |
| `WindowLeft` | `double?` | **【未実装】** | ウィンドウの左端X座標。`null` = 未保存。 |
| `WindowTop` | `double?` | **【未実装】** | ウィンドウの上端Y座標。`null` = 未保存。 |
| `WindowWidth` | `double?` | **【未実装】** | ウィンドウの幅。`null` = 未保存。 |
| `WindowHeight` | `double?` | **【未実装】** | ウィンドウの高さ。`null` = 未保存。 |

### 2.3 設定ファイルのJSON構造

```json
{
  "Profiles": [
    {
      "Id": "Default",
      "DisplayName": "仕事用",
      "IsVisible": true,
      "Order": 0,
      "IconPath": "C:\\Users\\...\\Google Profile Picture.png"
    },
    {
      "Id": "Profile 1",
      "DisplayName": "個人",
      "IsVisible": true,
      "Order": 1,
      "IconPath": ""
    }
  ],
  "WindowLeft": 100.0,
  "WindowTop": 100.0,
  "WindowWidth": 420.0,
  "WindowHeight": 500.0
}
```

> **注記**: `System.Text.Json` のデフォルト設定を使用するため、JSONのプロパティ名はC#のプロパティ名（PascalCase）と一致する。`IsRunning`、`Hwnd` は `[JsonIgnore]` 属性によりシリアライズ対象外。

## 3. サービス設計

### 3.1 IProfileDiscoveryService (プロファイル探索)
- `GetAvailableProfiles()`: Chrome の `User Data` フォルダ内の `Local State` を JSON 解析し、`profile.info_cache` から全プロファイルのIDと表示名を取得する。各プロファイルに対して `IIconService` でアイコンパスも設定する。
- **フォールバック**: `Local State` が存在しない、またはJSON解析に失敗した場合は空のリストを返す。

### 3.2 IIconService (アイコン取得)
- `GetIconPath(string profileId)`: 
    1. プロファイルフォルダ内の `Google Profile Picture.png` を優先的に探す。
    2. なければ `Google Profile.ico` を確認し、あればパスを返す。
    3. いずれもなければ空文字列を返す。`chrome.exe` のアイコンは使用しない。
- **キャッシュ**: キャッシュ機構は使用しない。プロファイルフォルダ内の画像ファイルのパスを直接返す方式とする。

### 3.3 ILauncherService (起動・フォーカス制御)
- `LaunchOrFocus(ProfileInfo profile)`:
    1. **キャッシュ検証**: `ProfileInfo.Hwnd` が有効か確認（`IsWindow` + `IsWindowVisible` + クラス名・プロセス名チェック）。有効ならフォーカスして終了。
    2. **起動判定**: `ProfileInfo.Id` に対応する `SingletonLock` ファイルの有無を確認。LOCKがなければ未起動とみなし、起動処理へ進む。
    3. **ウィンドウ探索**: LOCKがある場合、既存のChromeウィンドウを走査し、スコアリングにより最適なウィンドウを特定してフォーカスする（詳細は §3.3.1 参照）。
    4. **フォールバック起動**: ウィンドウが見つからない場合（LOCKの異常残存等）、新規起動処理へ進む。
    5. **新規起動**: `--profile-directory` 引数付きで Chrome を起動し、ウィンドウ差分検出で新規ウィンドウを捕捉する（詳細は §3.3.2 参照）。

#### 3.3.1 ウィンドウ探索のスコアリング
既存ウィンドウの中から対象プロファイルのウィンドウを特定する際、以下のスコアリングを適用する。

| 判定項目 | スコア | 説明 |
| :--- | :--- | :--- |
| ウィンドウタイトルマッチ | +50点 | タイトルにプロファイルの `DisplayName` が含まれている場合 |
| コマンドライン引数マッチ | +30点 | プロセスの引数に `--profile-directory` が一致する場合 |

- **採用閾値**: 合計スコアが **30点以上** のウィンドウを採用候補とする。
- **即決閾値**: 合計スコアが **80点以上** の場合、探索を即座に終了する。
- スコアが0以下のウィンドウは候補から除外する。

> **補足**: ウィンドウクラス名 (`Chrome_WidgetWin_1`) とプロセス名 (`chrome`) によるフィルタリングは、スコアリング以前の前提条件として全てのウィンドウに適用される。これにより、Electron系アプリ（Slack、VS Code、Discord等）の誤検出を防止する。

#### 3.3.2 差分検出による新規ウィンドウ捕捉
Chrome起動時に、プロセスの集約（既存プロセスへの吸収）が発生してもウィンドウを確実に捕捉するため、以下の2層の差分検出を行う。

**ウィンドウ差分検出（メイン）:**
1. 起動前に `EnumWindows` で現在のChromeウィンドウのハンドル一覧を保存。
2. 起動後、ポーリングで新規ウィンドウの出現を監視。
3. スナップショットに含まれない新規ハンドルを「今回起動したプロファイル」として紐付ける。

**プロセス差分検出（補助）:**
1. 起動前にChromeプロセスのPID一覧を保存。
2. 起動後に増加したPIDのうち、コマンドライン引数が対象プロファイルに一致するプロセスのウィンドウを特定。

**タイミングパラメータ:**
| パラメータ | 値 | 説明 |
| :--- | :--- | :--- |
| ポーリング間隔 | 500ms | 差分チェックの実行間隔 |
| 最大リトライ回数 | 15回 | 最大7.5秒間の監視 |
| タイムアウト後の挙動 | ログ出力のみ | アプリはフリーズせず通常状態に復帰 |

### 3.4 ISettingsService (設定管理)

#### 実装済みメソッド
- `LoadSettings()`: `settings.json` から `AppSettings` を読み込む。ファイルが存在しない場合はデフォルトの空設定を返す。
- `SaveSettings(AppSettings settings)`: `AppSettings` を JSON 形式で `settings.json` に保存する。

#### 【未実装】拡張メソッド（ウィンドウ位置管理）
プロファイル設定とウィンドウ位置を独立して保存・読み込みできるようにし、`SaveSettings()` でのプロファイル保存時にウィンドウ位置が上書き消失する問題を回避する。

- `LoadWindowPosition()`: `AppSettings` からウィンドウ位置（Left, Top, Width, Height）を読み込む。
- `SaveWindowPosition(double left, double top, double width, double height)`: 既存の設定をロードし、ウィンドウ位置のみを更新して保存する（プロファイル設定はそのまま保持）。

### 3.5 【未実装】WindowPositionHelper（座標バリデーション）
`Helpers/WindowPositionHelper.cs` に配置する静的ユーティリティクラス。UI非依存のため単体テスト可能。

- `IsPositionValid(left, top, width, height, screenLeft, screenTop, screenWidth, screenHeight)`: ウィンドウの一部が仮想スクリーン境界内に見えているかを判定。完全に画面外の場合のみ `false`。
- `IsSizeValid(width, height)`: サイズが最小閾値（幅 200px、高さ 150px）以上かを判定。

**座標系とDPI対応:**
- 保存・復元する座標はすべて **WPF DIP（Device Independent Pixel、論理ピクセル）** 単位とする。WPFの `Window.Left`/`Top`/`Width`/`Height` および `SystemParameters.VirtualScreen*` はいずれもDIP単位のため、変換は不要。
- DPI設定が変更された場合（例: 150%→100%）、以前の座標がスクリーン外になる可能性があるが、`IsPositionValid` のフォールバック条件でカバーされるため、DPI固有の追加ロジックは不要。

**仮想スクリーン境界の取得:**
- `IsPositionValid` の `screenLeft`/`screenTop`/`screenWidth`/`screenHeight` 引数には、呼び出し元（`MainWindow.xaml.cs`）で以下のWPF APIの値を渡す:
    - `SystemParameters.VirtualScreenLeft`
    - `SystemParameters.VirtualScreenTop`
    - `SystemParameters.VirtualScreenWidth`
    - `SystemParameters.VirtualScreenHeight`

## 4. UI設計 (WPF)

### 4.1 MainWindow (ランチャ画面)
- **デフォルトサイズ**: 幅 420px × 高さ 500px。
- **スタイル**: ダークテーマ (#0F0F0F) を基調としたモダンなデザイン。カード（アイテム）背景には視認性向上のため `#222222` を採用。
- **構成**:
    - `ListBox`: 各アイテムを角丸カード形式で表示。`ScrollViewer.CanContentScroll="False"` によるピクセル単位のスムーズスクロールに対応。
    - 各アイテム: アイコン、表示名を**左揃え**で配置。文字色は白 (`#FFFFFF`)。ホバー・クリック時の視覚効果あり。
    - タイトル表示の廃止: レイアウトのコンパクト化のため、「Chrome Launcher」等のタイトル表示を削除。
    - 下部: 「Settings」ボタン（丸みのあるモダンなデザイン）。
    - **DimmerOverlay**: 設定画面表示中にメイン画面全体を半透明（黒）で覆い、グレーアウト状態を表現。
- **【未実装】ウィンドウ位置の復元・保存**:
    - `MainWindow.xaml` の `WindowStartupLocation="CenterScreen"` を削除し、コードビハインドで制御する。
    - `OnSourceInitialized` で `ISettingsService.LoadWindowPosition()` を呼び出し、保存位置を復元。復元時は `WindowStartupLocation = Manual` を設定してから座標を適用する。
    - `OnClosing` で `WindowState == Normal` の場合のみ `ISettingsService.SaveWindowPosition()` を呼び出し、現在位置を保存。
    - `WindowPositionHelper` でバリデーションを実行し、不正な値の場合は `WindowStartupLocation` を `Manual` にセットせず、XAMLのデフォルト値 `CenterScreen` を維持することでプライマリモニターの中央にフォールバックする。
    - **前提条件**: `DataContext` はXAML内で `<Window.DataContext>` として設定されるため、`InitializeComponent()` 完了後（= `OnSourceInitialized` 時点）で `MainViewModel` 経由の `ISettingsService` へのアクセスが可能である。

### 4.2 SettingsWindow (設定画面)
- **スタイル**: メイン画面と統一したダークテーマのカードレイアウト。
- **構成**:
    - 各アイテム左側にドラッグ用のハンドル (`☰`) を配置。ドラッグ中はドロップ先のアイテムを半透明 (Opacity 0.4) にすることでドロップ位置を分かりやすく表示。
    - 各プロファイルに表示/非表示を切り替えるトグルスイッチ (CheckBoxスタイル) を配置。
    - プロファイル名の下にフォルダ名 (`Id`) を併記。
    - 各アイテムに 「OPEN (📂)」 ボタンを配置し、直接フォルダを開く機能を提供。
    - 不要なラベル（「NAVIGATION MODULES」等）を削除したシンプルなレイアウト。
    - 保存・キャンセルボタン（青と黒のボタン）。

## 5. 処理フロー

### 5.1 起動時処理
1. `AppSettings` を読み込む。
2. `IProfileDiscoveryService` で Chrome フォルダを走査し、`AppSettings` にない新規プロファイルがあれば追加（データマージ）。
3. `IIconService` で全プロファイルのアイコンパスを設定。
4. マージ後のプロファイル一覧を `settings.json` に保存（新規プロファイルの永続化）。
5. 表示対象（`IsVisible == true`）のプロファイルのみをメイン画面に表示。
6. **【未実装】** ウィンドウ位置を復元（`WindowPositionHelper` によるバリデーション → 有効なら座標適用、無効なら画面中央）。

### 5.1a 【未実装】終了時処理
1. `WindowState` が `Normal` であれば、現在のウィンドウ位置・サイズを `ISettingsService.SaveWindowPosition()` で保存。
2. 最大化・最小化状態の場合は保存をスキップ（前回の通常状態の値を維持）。

### 5.1b 単一インスタンス制御（二重起動防止）
アプリ起動時 (`App.xaml.cs` の `OnStartup`) に、以下の処理を行う。

1. **Mutex チェック**: `ChromeProfileLauncher-SingleInstance-Mutex` という名前の Mutex を作成し、所有権を確認。
2. **既存インスタンス検出**: Mutex の所有権が取得できない場合、既に別インスタンスが起動していると判断。
3. **既存ウィンドウの活性化**: `NativeMethods.FindWindow` で "Chrome Profile Launcher" タイトルのウィンドウを探索。
4. **状態復元と前面化**: 
    - ウィンドウが見つかった場合、`IsIconic` で最小化状態か確認。
    - 最小化されていれば `ShowWindow(SW_RESTORE)` で復元。
    - `SetForegroundWindow` で最前面に表示。
5. **終了**: 活性化処理後、後から起動したインスタンスを `Shutdown()` する。

### 5.2 起動・フォーカス処理
1. **キャッシュ検証**: 保持している `Hwnd` が有効か確認（`IsWindow` + `IsWindowVisible` + クラス名・プロセス名チェック）。有効ならフォーカスして終了。
2. **LOCK確認**: `SingletonLock` ファイルの存在を確認。LOCKなしなら手順5へ。
3. **ウィンドウ探索**: LOCKがあれば、全Chromeウィンドウをスコアリングで走査し、最適なウィンドウを特定。見つかればフォーカスして終了。
4. **フォールバック**: LOCKはあるがウィンドウが見つからない場合（異常残存等）、新規起動へ。
5. **スナップショット**: 起動前に現在のウィンドウ一覧（およびPID一覧）を保存。
6. **起動**: `--profile-directory` 指定でプロセスを起動。
7. **差分検出**: ポーリング（500ms × 最大15回 = 7.5秒）で新しいウィンドウまたはプロセスの出現を監視。
8. **登録**: 新規ウィンドウを特定し、`ProfileInfo.Hwnd` を更新してフォーカス。

### 5.3 フォーカス制御の詳細
ウィンドウを確実に最前面に移動させるため、以下の手順で `SetForegroundWindow` を実行する。

1. `ShowWindow(hwnd, SW_RESTORE)` で最小化状態を解除。
2. `GetForegroundWindow()` で現在のフォアグラウンドウィンドウのスレッドIDを取得。
3. フォアグラウンドスレッドと対象スレッドが異なる場合、`AttachThreadInput` でスレッドをアタッチ。
4. `SetForegroundWindow` を実行。
5. スレッドアタッチを解除。

## 6. エラーハンドリング

### 6.1 ログ出力
- **出力先**: `%AppData%\ChromeProfileLauncher\debug.log`
- **形式**: `[yyyy-MM-dd HH:mm:ss] [LEVEL] message`
- **レベル**: `INFO` / `ERROR`
- コンソール出力（`Console.WriteLine`）およびデバッグ出力（`Debug.WriteLine`）にも同時出力。

### 6.2 エラー時の方針
- **基本方針**: エラー発生時もアプリケーションの動作を継続する。
- **Chrome未発見時**: `FileNotFoundException` をスローし、呼び出し元で処理。
- **設定ファイル読み込み失敗**: デフォルトの空設定 (`new AppSettings()`) を返す。
- **設定ファイル書き込み失敗**: `MessageBox` でユーザーに通知。
- **プロファイル探索失敗**: 空のリストを返す（JSON解析エラー等）。

### 6.3 Chromeの自動検出
以下の順序で `chrome.exe` のパスを検索する。いずれにも見つからない場合は空文字列となり、起動時に `FileNotFoundException` が発生する。

1. `%ProgramFiles%\Google\Chrome\Application\chrome.exe`
2. `%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe`

## 7. 技術スタック・ライブラリ

- **フレームワーク**: .NET 10.0 (WPF)
- **UI フレームワーク**: WPF
- **JSON 解析**: `System.Text.Json`
- **Win32 API**: `P/Invoke` (user32.dll: `SetForegroundWindow`, `ShowWindow`, `IsWindow`, `IsWindowVisible`, `EnumWindows`, `GetWindowText`, `GetClassName`, `GetForegroundWindow`, `AttachThreadInput`, `GetWindowThreadProcessId`, `FindWindow`, `IsIconic` / kernel32.dll: `GetCurrentThreadId`)
- **プロセス監視**: `System.Management` (WMI — コマンドライン引数の取得に使用)

### 7.1 P/Invoke 定義 (NativeMethods)
`App.xaml.cs` 内に定義される、二重起動防止用のネイティブメソッド。

- `FindWindow`: クラス名またはウィンドウタイトルからウィンドウハンドルを取得。
- `SetForegroundWindow`: 指定したウィンドウをフォアグラウンドにする。
- `ShowWindow`: ウィンドウの表示状態を設定（`SW_RESTORE = 9` を使用）。
- `IsIconic`: ウィンドウが最小化されているか判定。

## 8. 配布・ビルド設計 (Deployment)

本アプリは、仕様書の「配布容易なアプリ」を実現するため、以下のビルド構成を採用する。

### 8.1 単一EXE形式 (Self-contained Single-file)
- **方式**: `.NET Runtime` を含めた単一の実行ファイル (`.exe`) として出力。
- **メリット**: ターゲットPCに .NET 10 がインストールされていなくても、EXE単体で即座に動作する。
- **ターゲットアーキテクチャ**: `win-x64` (Windows 10/11 64bit)
- **リリースビルド設定**:
    - `PublishSingleFile`: `true` (単一ファイル化)
    - `SelfContained`: `true` (ランタイム同梱)
    - `PublishReadyToRun`: `true` (起動速度の最適化)

> **注記**: 開発中は `PublishSingleFile` および `SelfContained` を `false` に設定しており、デバッグビルドではランタイム同梱は行わない。リリース時に `dotnet publish` コマンドで上記設定を適用する。

### 8.2 配布ファイル
- `ChromeProfileLauncher.exe` (約100MB)
- インストーラーは使用せず、実行ファイルを任意のフォルダ（デスクトップやドキュメント等）に配置するだけで使用可能とする。
