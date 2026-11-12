# スマート アプリ コントロール対応（Microsoft Store 配布）

Issue: https://github.com/ikaken/chrome-profile-launcher/issues/89

## 背景 / 目的

Windows 11 22H2 以降の「スマート アプリ コントロール（SAC）」は、Microsoft のクラウド評価で安全と判断できず、かつ有効なコード署名のないアプリを **実行ブロック** する。SmartScreen と異なり「詳細情報 → 実行」のような回避手段はなく、アプリ単位の許可もできない（ユーザー側の回避策は SAC 自体を OFF にすることのみで、一度 OFF にすると Windows 再インストールまで再有効化できない）。

現在のリリース成果物（Velopack の Setup.exe / Update.exe / ChromeProfileLauncher.exe / 各 DLL）はすべて未署名のため、SAC が「オン」の PC ではブロックされる。

根本対応はコード署名だが、Azure Artifact Signing（約 $10/月）や他社 CA 証明書（年 $200〜）はコストがかかる。
**Microsoft Store 経由で配布すると Microsoft がパッケージに署名する**ため、SAC / SmartScreen を無料で通過できる。2025 年 9 月より個人開発者の Store 登録料は無料化されている（要: 政府発行 ID による本人確認）。

本 Issue では、既存の Velopack 配布（GitHub Releases）を維持したまま、**Store 提出用の MSIX パッケージをリリースパイプラインで生成**し、MSIX として動作した際にアプリが正しく振る舞うようにする。

## 対応方針（設計）

### 1. 配布チャネル

| チャネル | 形式 | 署名 | 自動更新 | 対象 |
|---|---|---|---|---|
| GitHub Releases（既存） | Velopack Setup.exe | なし | Velopack | SAC が無効/評価モードの PC |
| Microsoft Store（新規） | MSIX | Microsoft | Store | SAC が有効な PC を含む全ユーザー（推奨） |

### 2. MSIX パッケージ生成（GitHub Actions）

`.github/workflows/release.yml` に MSIX 生成ステップを追加する。

- `dotnet publish` の出力（self-contained, win-x64）をそのまま MSIX の内容物とする。
- `Packaging/AppxManifest.xml`（テンプレート）と `Packaging/Assets/*.png`（タイル画像）を publish ディレクトリにコピーし、Windows SDK 同梱の `makeappx.exe pack` で `.msix` を生成する。
- マニフェストの可変値はワークフローで置換する:
  - `{{VERSION}}`: タグ `vX.Y.Z` → `X.Y.Z.0`（Store は 4 桁目 0 を要求）
  - `{{IDENTITY_NAME}}` / `{{PUBLISHER}}` / `{{PUBLISHER_DISPLAY_NAME}}`: Partner Center で予約した値。GitHub リポジトリ変数 `MSIX_IDENTITY_NAME` / `MSIX_PUBLISHER` / `MSIX_PUBLISHER_DISPLAY_NAME` から取得。
- プレリリースタグ（`-beta` / `-rc`）では MSIX を生成しない（MSIX バージョンに文字列を含められないため）。
- リポジトリ変数が未設定の場合は MSIX ステップをスキップする（Velopack リリースには影響させない）。
- 生成した `.msix` は **署名なし** のため GitHub Release の添付ではなく **workflow artifact** としてアップロードする（Partner Center へ手動アップロード）。
- `dotnet publish` に `/p:Version=$version` を付与し、アセンブリ情報バージョンをタグと一致させる（両チャネル共通）。

マニフェスト要点:
- `EntryPoint="Windows.FullTrustApplication"`、`Executable="ChromeProfileLauncher.exe"`
- `rescap:runFullTrust` ケイパビリティ
- `TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0"`
- `uap5:StartupTask`（TaskId=`ChromeProfileLauncherStartup`, `Enabled="false"`）: 下記 3-3 で使用
- 画像: `Square44x44Logo.png`, `Square150x150Logo.png`, `StoreLogo.png`（`Assets/app.ico` から生成しコミット）

### 3. アプリ側の MSIX（パッケージ実行）対応

パッケージ化されて動作しているかを `Helpers/PackagedAppHelper.IsPackaged` で判定する（`kernel32!GetCurrentPackageFullName` が `APPMODEL_ERROR_NO_PACKAGE`(15700) を返せば非パッケージ）。

#### 3-1. 自動更新（Velopack）の無効化
- `UpdateService.CheckForUpdatesAsync()`: パッケージ実行時は即 `null` を返す（更新は Store が担う）。
- `UpdateService.GetCurrentVersion()`: Velopack 未インストール時（`CurrentVersion == null`）はアセンブリ情報バージョンへフォールバックする（現状は `"0.0.0"` 固定）。
- `SettingsViewModel.IsAutoUpdateSettingVisible`（新規）: パッケージ実行時 `false`。`SettingsWindow.xaml` の自動更新チェックボックス行を `Visibility` バインドで非表示にする。
- `App.OnStartup` の `VelopackApp.Build().Run()` は非インストール環境では no-op のため変更不要。

#### 3-2. 初回起動判定
- 現状は環境変数 `VELOPACK_FIRSTRUN` のみで判定しており、Store 版では初回セットアップが表示されない。
- `ISettingsService.SettingsFileExists()` を追加し、`isFirstRun = VELOPACK_FIRSTRUN || !SettingsFileExists()` とする（既存ユーザーは settings.json があるため影響なし）。

#### 3-3. スタートアップ登録
- 現状の `StartupService` は `HKCU\...\Run` へ書き込むが、MSIX ではレジストリが仮想化されるためログオン時に起動されない。
- `IStartupService` の実装 `PackagedStartupService`（新規）を追加し、`Windows.ApplicationModel.StartupTask` API（`GetAsync` → `RequestEnableAsync` / `Disable`）を使用する。
- ファクトリ `StartupServiceFactory.Create()` で `IsPackaged` に応じて実装を切り替え、`App.xaml.cs` / `SettingsViewModel` の `new StartupService()` を置き換える。
- WinRT API 使用のため `TargetFramework` を `net10.0-windows10.0.17763.0` に変更する（テストプロジェクトも同様）。これに伴い publish 出力パスが `bin/Release/net10.0-windows10.0.17763.0/win-x64/publish` に変わるため、`release.yml` / `docs/release_guide.md` を更新する。
- `StartupTask` の状態が `DisabledByUser` の場合は `IsRegistered()=false` を返し、`Register()` は何もしない（ユーザーがタスク マネージャーで無効化したケース）。

### 4. ドキュメント

- `README.md`（日英）: インストール方法に「Microsoft Store から入手（推奨）」を追加。SmartScreen 警告セクションに **SAC の説明**（回避不可・Store 版の利用を案内）を追記。Store 公開後にリンクを差し替える。
- `docs/store_publishing_guide.md`（新規）: Partner Center 個人アカウント登録（無料・本人確認）、アプリ名予約、Identity 値の取得、GitHub リポジトリ変数の設定、artifact の取得と提出、審査の流れ。
- `docs/release_manual.md` / `docs/release_guide.md`: MSIX 生成と Store 提出の手順を追記。
- `docs/design_document.md` 8 章: Store 配布方式と `IsPackaged` 分岐を追記。

## 変更内容

- `.github/workflows/release.yml`: `/p:Version` 付与、MSIX 生成・artifact アップロード
- `Packaging/AppxManifest.xml`, `Packaging/Assets/*.png`: 新規
- `ChromeProfileLauncher.csproj`, `ChromeProfileLauncher.Tests/*.csproj`: TFM 変更
- `Helpers/PackagedAppHelper.cs`: 新規
- `Services/UpdateService.cs`: パッケージ時スキップ・バージョンフォールバック
- `Services/SettingsService.cs`: `SettingsFileExists()` 追加
- `Services/PackagedStartupService.cs`, `Services/StartupServiceFactory.cs`: 新規
- `App.xaml.cs`: 初回判定・StartupService 生成の置換
- `ViewModels/SettingsViewModel.cs`, `Views/SettingsWindow.xaml`: 自動更新設定の表示切替
- `README.md`, `docs/*`: 上記 4

## テスト

- `SettingsViewModelTests`: `IsAutoUpdateSettingVisible` がパッケージ判定に従うこと（判定値を注入可能にする）
- `MainViewModelTests` / `UpdateService`: パッケージ時に `CheckForUpdatesAsync` が `null` を返すこと、`GetCurrentVersion` のフォールバック
- `SettingsServiceTests`: `SettingsFileExists()`
- 初回起動判定ロジックを `App` から静的ヘルパーへ切り出し、`VELOPACK_FIRSTRUN` と設定ファイル有無の組み合わせをテスト
- 手動: ローカルで `makeappx pack` → 自己署名証明書でテスト署名してインストールし、起動・初回セットアップ・設定画面（自動更新行の非表示）・スタートアップ登録を確認

## 影響範囲

- 既存 Velopack ユーザー: `GetCurrentVersion` のフォールバック以外に挙動変更なし。初回判定は settings.json が存在するため従来通り。
- Store ユーザー: 新規。更新は Store 経由、スタートアップは StartupTask 経由。
- 開発者: TFM 変更により publish 出力パスが変わる。

## 備考 / ユーザー側で必要な作業

1. https://storedeveloper.microsoft.com で個人開発者アカウントを作成（無料、政府発行 ID + セルフィーによる本人確認、MFA 必須）。
2. Partner Center でアプリ名「Chrome Profile Launcher」を予約し、「製品 ID」ページから Package/Identity/Name・Publisher・PublisherDisplayName を取得。
3. GitHub リポジトリ変数 `MSIX_IDENTITY_NAME` / `MSIX_PUBLISHER` / `MSIX_PUBLISHER_DISPLAY_NAME` を設定。
4. タグ push 後、Actions の artifact から `.msix` をダウンロードして Partner Center に提出（ストア掲載情報・スクリーンショット・プライバシーポリシー URL が必要）。

将来的には `microsoft/setup-msstore-cli` による提出自動化が可能（Azure AD アプリ登録が必要なため本 Issue では対象外）。

## 見送った案

- 自己署名証明書: 信頼されたルートにチェーンしないため SAC は通らない。
- SignPath Foundation（OSS 無料署名）: 審査・手動承認運用が必要で、Velopack 生成物の署名可否が不明確。
- Azure Artifact Signing: 有料（約 $10/月）のため今回は対象外。将来導入する場合は `vpk pack --azureTrustedSignFile` で対応可能。
