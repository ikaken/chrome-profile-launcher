# Chromeプロファイルランチャ リリース・配布ガイド

本プロジェクトを GitHub で公開し、ユーザーに配布するための手順書です。

## 1. ビルド作業 (作成者)

配布用の単一EXEファイルを作成します。

```bash
dotnet publish -c Release
```

生成された以下のファイルを配布用パッケージとして使用します。
- `bin\Release\net10.0-windows\win-x64\publish\ChromeProfileLauncher.exe`

## 2. GitHub リリースの作成手順

1. **GitHub リポジトリ** にブラウザでアクセスします。
2. 右サイドメニューの **"Releases"** セクションにある **"Create a new release"** をクリックします。
3. **"Choose a tag"** をクリックし、新しいバージョン番号を入力します（例: `v1.0.0`）。
4. **"Release title"** を入力します（例: `Initial Release v1.0.0`）。
5. **"Description"** に今回の更新内容を記述します。
6. **"Attach binaries..."** のエリアに、作成した `ChromeProfileLauncher.exe` をドラッグ＆ドロップします。
7. **"Publish release"** をクリックして公開完了です。

## 3. ユーザーへの案内

配布ページ（GitHub の Release ページ）の URL をユーザーに伝えます。
ユーザーは `ChromeProfileLauncher.exe` をクリックしてダウンロードし、即座に利用を開始できます。

### ユーザー動作環境
- Windows 10 / 11 (64bit)
- 追加の .NET ランタイムインストールは不要（EXEに同梱済み）

## 4. 注意事項

- **セキュリティ警告**: 署名（コードサイニング証明書）がない EXE の場合、Windows SmartScreen により「PCが保護されました」という警告が出ることがあります。その場合は「詳細情報」→「実行」をクリックするようユーザーに案内してください。
- **設定の引き継ぎ**: 設定ファイルは `%AppData%` に保存されるため、EXEを入れ替えても設定は維持されます。
