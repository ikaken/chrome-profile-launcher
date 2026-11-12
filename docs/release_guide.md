# Chromeプロファイルランチャ リリース・配布ガイド

本プロジェクトを配布するための手順書です。本番リリースはGitHub ActionsとVelopackを使用します。

## 1. 事前準備

- Windows 10 / 11（64bit）
- .NET 10.0 SDK
- GitHub CLI

ローカルでパッケージを検証する場合は、アプリが参照するSDKと同じバージョンのVelopack CLIを使用します。

```powershell
dotnet tool install -g vpk --version 0.0.1251
```

## 2. ローカルビルド

.NETランタイムを含むWindows x64向け成果物を生成します。

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishReadyToRun=true
```

生成パス: `bin\Release\net10.0-windows10.0.17763.0\win-x64\publish\`

## 3. ローカルパッケージ検証

```powershell
vpk pack --packId ChromeProfileLauncher --packVersion 1.0.0 --packDir ./bin/Release/net10.0-windows10.0.17763.0/win-x64/publish --outputDir ./releases --icon ./Assets/setup-icon.ico
```

生成されたSetupファイルを.NET Desktop RuntimeがインストールされていないWindows x64環境で実行し、追加ダウンロードなしで起動することを確認します。

## 4. GitHubリリース

1. `develop`から`main`へのリリース対象変更を確定します。
2. 重複しない`v*`形式のタグを作成します。
3. タグをpushすると`.github/workflows/release.yml`がself-contained publish、Velopackパッケージ作成、GitHub Releaseへの成果物添付を実行します。リポジトリ変数 `MSIX_IDENTITY_NAME` 等が設定済みの場合、Store提出用の`.msix`も生成されworkflow artifact（`ChromeProfileLauncher-msix-X.Y.Z`）としてアップロードされます。
4. GitHub Actionsの成功とRelease成果物を確認します。

## 4.1 Microsoft Store への提出

生成された`.msix`をMicrosoft Storeに提出する手順は [store_publishing_guide.md](store_publishing_guide.md) を参照してください。

## 5. ユーザー動作環境

- Windows 10 / 11（64bit）
- .NET Desktop Runtimeの事前インストールは不要

## 6. 旧Inno Setupスクリプト

`installer/setup.iss`は旧手動インストーラー用であり、本番リリースでは使用しません。保守確認に使用する場合は、self-contained publish後にpublishディレクトリ全体を格納します。
