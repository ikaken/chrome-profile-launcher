# Issue Handling: Generalist Guidelines

## 🤖 役割
軽量モデル（gemini-3.1-flash-lite-preview）を活用し、速度と正確性が求められる定型作業、Git操作、および検証を担当します。

## 📝 担当フェーズ詳細

### Step 0: 環境チェック
- `git status` で作業ディレクトリがクリーンか確認。
- `gh auth status` でPR作成が可能か確認。
- **重要 (Windows環境)**: 日本語の文字化けを防ぐため、`gh` コマンド実行前に必ず `chcp 65001` を実行するか、PowerShell のエンコーディングを UTF-8 に設定してください。
- **最新性の担保**: Issueの一覧や詳細を取得する際は、必ずGitHub CLI (`gh issue list`, `gh issue view`) を使用してGitHubから最新情報をフェッチしてください。ローカルの `issues.json` 等の古いキャッシュファイルに依存しないこと。

### Step 2-3: 準備
- `[CONVENTIONS.md](./CONVENTIONS.md)` に従い、作業用ブランチと `changes` ファイルを正確に作成する。

### Step 6-8: コミット・更新・テスト
- **コミット**: Architectの実装単位ごとに、[CONVENTIONS.md](./CONVENTIONS.md) の規約に従ってコミットを行う。
- **追記**: 変更内容を `docs/changes/` に随時反映する。
- **テスト**: 修正箇所の動作確認、ユニットテストの実行、リントエラーの有無をチェックする。

### Step 10-11: 公開
- 作業ブランチをリモートにPushする。
- ユーザーに代わって（または指示に従って）GitHub CLI で Pull Request を作成する。

---

## 🛠 推奨コマンド
- **Git**: `git checkout -b ...`, `git commit -m "..."`, `git push origin ...`
- **GitHub CLI (Windows)**:
  - Issue一覧取得（UTF-8対策済み）: `powershell -Command "$OutputEncoding = [System.Text.Encoding]::UTF8; gh issue list -s open --limit 20"`
  - Issue詳細取得: `chcp 65001; gh issue view {number}`
  - PR作成: `chcp 65001; gh pr create --title "..." --body "..."`
- **検証**: `npm test`, `dotnet test` など、プロジェクトに応じたテストコマンド。
