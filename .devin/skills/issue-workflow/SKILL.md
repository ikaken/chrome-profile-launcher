---
name: issue-workflow
description: GitHub Issue対応ワークフロー。設計レビューと実装確認の承認ゲートを必ず設ける。
---

# Issue Handling Workflow

## 承認ゲート（厳格）
- **設計レビュー**: 実装前に設計方針を提示し、ユーザー承認を得る。
- **実装確認**: テスト通過後、ユーザーに動作確認を依頼する。
- **mainへのプッシュ**: 承認なしに `main` へプッシュしない。

## 実行トリガー
- 「イシューの一覧を取得してください」
- 「Issue #xx に着手してください」
- 「Issue #xx の調査と修正をお願いします」

## フロー

### Phase 1: 初期化
1. **Step 0 環境チェック**: `git status`, `gh auth status`。`gh` 実行前に `chcp 65001` または UTF-8 設定。GitHub CLI で最新 Issue 情報を取得し、ローカルキャッシュに依存しない。**Issue 一覧は必ず `--json number,title,state,createdAt,updatedAt,url` で取得し、`createdAt` / `updatedAt` を確認して最新の Issue かどうかを検証すること。必要に応じて `-S \"sort:updated-desc\"` または `-S \"sort:created-desc\"` で検索ソートを指定する。**
2. **Step 1 Issue分析**: コードを精査し根本原因を特定。不明点は実装前に質問する。

### Phase 2: 準備
3. **Step 2 ブランチ作成**: `feature/{Issue番号}-{内容}`（例: `feature/012-profile-sorting`）。
4. **Step 3 changes ファイル作成**: `docs/changes/{Issue番号}-{内容}.md`（テンプレートは [CONVENTIONS.md](./CONVENTIONS.md)）。

### Phase 3: 設計
5. **Step 4 設計方針策定**: 変更箇所を `docs/changes/` に記述し、アーキテクチャ違反をチェックする。
6. **Step 4.5 設計レビュー（承認必須）**: 承認を得るまで実装ツールを実行しない。

### Phase 4: 実装
7. **Step 5-8 実装・コミット・テスト**: 小さな単位でコミット。CONVENTIONS.md のコミット規約に従い、テストを実行する。
8. **Step 8.5 実装確認（承認必須）**: ユーザーに動作確認を依頼する。

### Phase 5: 完了
9. **Step 9 主要ドキュメント反映**: 仕様書等を最新の実装と整合させる。
10. **Step 10 PR作成提案**: ユーザーに「PRを作成しますか？」と確認する。
11. **Step 11 PR作成**: 承認後、Push して `gh pr create` を実行。`Fixes #{Issue番号}`、changes ファイルリンク、動作確認エビデンスを必ず含める。
12. **Step 12 マージ後整理**: `gh pr view` で確認、`git checkout main && git pull origin main`、作業ブランチ削除（任意）、`gh issue close <number>`。

## ツール実行時の注意
- コマンドは簡潔に、1 つの目的ごとに個別に実行する。長いコマンド文字列や過度な `&&` / `;` 連鎖は避ける。
- 複数の情報が必要な場合は、複数回の `run_command` を使用するか、ユーザーに結果を確認しながら進める。
- 詳細なコマンド例は [CONVENTIONS.md](./CONVENTIONS.md) を参照。

## エラー・フォールバック
- `429` 等が発生したら即座に中断し、完了ステップと未完了タスクを報告する。
- ツール呼び出しエラー（トークン超過等）が発生した場合は、コマンドをさらに短くして再試行する。
