# Chromeプロファイルランチャ リリース・配布ガイド

本プロジェクトを配布するための手順書です。

## 1. ビルド作業 (作成者)

### 1.1 アプリケーションのビルド
配布用のバイナリを作成します。

```bash
dotnet publish -c Release -r win-x64 --no-self-contained
```

生成パス: `bin\Release\net10.0-windows\win-x64\publish\`

### 1.2 インストーラーの作成
[Inno Setup](https://jrsoftware.org/isdl.php) を使用してインストーラーを作成します。

1. Inno Setup Compiler を開きます。
2. `installer\setup.iss` を読み込みます。
3. **Build** > **Compile** (Ctrl+F9) を実行します。
4. `installer\Output\ChromeProfileLauncherSetup.exe` が生成されます。

## 2. GitHub リリースの作成手順

1. **GitHub リポジトリ** にアクセスします。
2. **"Releases"** > **"Create a new release"** をクリックします。
3. タグを入力します（例: `v1.1.0`）。
4. リリース名を入力します（例: `Release v1.1.0`）。
5. `installer\Output\ChromeProfileLauncherSetup.exe` をアップロードします。
6. **"Publish release"** をクリックします。

## 3. ユーザーへの案内

ユーザーはインストーラーを実行することで、以下の恩恵を受けられます。
- スタートメニューへのショートカット追加。
- デスクトップアイコンの作成（選択可能）。
- アプリケーションの簡単な削除（アンインストーラー）。

### ユーザー動作環境
- Windows 10 / 11 (64bit)
- **.NET 10.0 Desktop Runtime** が必要です。
  - インストールされていない場合、実行時にダウンロードを促すダイアログが表示されます。

## 4. 注意事項

- **セキュリティ警告**: 署名がないため Windows SmartScreen の警告が出ることがあります。
- **設定の維持**: アンインストールしても `%AppData%\ChromeProfileLauncher\settings.json` は削除されないため、再インストール時に設定が引き継がれます。
