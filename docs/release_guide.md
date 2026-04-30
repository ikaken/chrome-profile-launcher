# Chromeプロファイルランチャ リリース・配布ガイド

本プロジェクトを配布するための手順書です。

## 1. 事前準備 (環境構築)

インストーラーを作成するには **Inno Setup 6** が必要です。

```powershell
winget install JRSoftware.InnoSetup
```

※ インストールパスが `C:\Users\[ユーザー名]\AppData\Local\Programs\Inno Setup 6\` になることを想定しています。

## 2. ビルド作業 (作成者)

### 2.1 アプリケーションのビルド
配布用のバイナリを作成します。

```powershell
dotnet publish -c Release -r win-x64 --no-self-contained
```

生成パス: `bin\Release\net10.0-windows\win-x64\publish\`

### 2.2 インストーラーの作成
Inno Setup Compiler を使用してインストーラーを作成します。

**GUIで行う場合:**
1. `installer\setup.iss` を Inno Setup で開きます。
2. **Build** > **Compile** (Ctrl+F9) を実行します。

**コマンドラインで行う場合:**
```powershell
& "C:\Users\ikaken\AppData\Local\Programs\Inno Setup 6\ISCC.exe" installer/setup.iss
```

生成ファイル: `installer\Output\ChromeProfileLauncherSetup.exe`

## 3. GitHub リリースの作成手順

1. **GitHub リポジトリ** にアクセスします。
2. **"Releases"** > **"Create a new release"** をクリックします。
3. タグ・リリース名を入力し、`ChromeProfileLauncherSetup.exe` をアップロードして公開します。

## 4. ユーザー動作環境
- Windows 10 / 11 (64bit)
- **.NET 10.0 Desktop Runtime** が必要です。

## 5. 開発時のテスト手順
インストーラーの動作確認は、プロジェクト内のテストディレクトリへのサイレントインストールで実施可能です。
```powershell
./installer/Output/ChromeProfileLauncherSetup.exe /VERYSILENT /DIR="test_install"
```
確認後、`test_install\unins000.exe /VERYSILENT` でクリーンアップしてください。
