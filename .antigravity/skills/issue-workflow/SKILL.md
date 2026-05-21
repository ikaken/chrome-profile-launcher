---
name: issue-workflow
description: GitHubのIssue対応を行う際の、設計・実装・ドキュメント・Git運用の標準フローを実行します。（詳細な規約、最適化、エラー対応は別ファイルに分割されています）
---

# AI Agent Skill: Issue Handling Workflow (Entry Point)

## 🎯 目的
AIエージェントがGitHubのIssue対応を行う際の、設計・実装・ドキュメント・Git運用の標準フローを定義します。品質とトレーサビリティを担保するため、このフローを厳密に遵守してください。

## 📝 トリガー
ユーザーから「Issue番号 #◯◯ に対応して」または「イシューの対応を開始して」と指示された場合に、このスキルを起動・実行します。

## 🛑 基本原則（絶対に守ること）
1. **直接コミットの禁止**: `main` ブランチへ直接コミットしないこと。
2. **Issue単位の管理**: 変更は必ずIssue単位の作業ブランチで管理すること。（命名規則：[CONVENTIONS.md](./CONVENTIONS.md)）
3. **ドキュメントの同期**: 主要ドキュメントは常に最新の実装状態と一致させること。
4. **履歴の保存**: 設計の判断やトレードオフは必ず `docs/changes/` のドキュメントに記録すること。

---

## 🛠 前提環境（Prerequisites）
本スキルを実行・運用するにあたり、以下の環境が整っていることを確認してください。
* **Git**: バージョン管理、初期設定が完了していること。
* **GitHub CLI (`gh`)**: 認証済みであること。
* **Windows環境での日本語文字化け防止**: `gh` コマンド実行前に必ず `chcp 65001` を実行するか、PowerShell のエンコーディングを UTF-8 に設定してください。

---

## 🔄 実行フロー（ステップ・バイ・ステップ）
以下のステップを順番に実行し、各ステップが完了するごとにユーザーに状況を報告してください。詳細な作業最適化については、[OPTIMIZATION.md](./OPTIMIZATION.md) を参照して作業を分散させることができます。

### Step 0: 環境チェック【自動実行】
1. **Git の確認**: `git --version`, `git config user.name`, `git config user.email`, `git remote -v`
2. **GitHub CLI の確認**: `chcp 65001; gh auth status`
3. **リポジトリ状態の確認**: `git status`, `git branch --show-current`
   - 未コミットの変更がある場合や、想定外のブランチにいる場合はユーザーに報告し、指示を仰ぐ。

### Step 1: Issue内容の確認と明確化
1. 指定されたIssueの内容を読み込む。
   ```powershell
   chcp 65001; gh issue view {Issue番号}
   ```
2. 要件を理解し、不明点や考慮すべきエッジケースがあれば洗い出す。（必要に応じて `research` サブエージェントを活用：[OPTIMIZATION.md](./OPTIMIZATION.md)）
3. 不明点があればユーザーに質問して仕様を確定させる。

### Step 2: 作業用ブランチの作成
1. [CONVENTIONS.md](./CONVENTIONS.md) の規則に従い、ブランチを作成してチェックアウトする。
   *(例: `git checkout -b feature/012-profile-sort`)*

### Step 3: 変更履歴（changes）ドキュメントの初期化【必須】
1. `docs/changes/` ディレクトリに `{issue番号}-{内容の英単語}.md` を作成する。
2. [CONVENTIONS.md](./CONVENTIONS.md) にある変更履歴テンプレートを展開し、初期状態を記述する。

### Step 4: 設計と方針の策定
1. 実装方針を決定し、Step 3で作成した `changes` ファイルに記録する。
2. アーキテクチャ変更がある場合は、事前に `docs/design_document.md` などの更新案を検討する。

### Step 4.5: 人間による設計レビュー（必須）
1. ユーザーに対して、決定した設計方針と `docs/changes/` の記述内容を報告し、確認を求める。
2. **ユーザーから「実装開始」または「OK」の指示が出るまで、絶対に次のステップへ進んではならない。設計承認前の実装ツール実行は厳格に禁止されます。**

### Step 5: 実装
1. 決定した設計方針に基づいてコードを変更する。
2. 変更はなるべく小さく分割し、意味のある単位で進める。

### Step 6: コミット
1. 実装がひと段落するごとにコミットする。（prefix規則：[CONVENTIONS.md](./CONVENTIONS.md)）
   ```bash
   git add .
   git commit -m "{prefix}: {日本語での変更内容}"
   ```

### Step 7: 実装中のドキュメント随時更新
1. 実装中に生じた設計変更や重要な判断は、必ず `changes` ファイルに追記する。

### Step 8: テスト・動作確認
1. 変更した機能の動作確認を行う。
2. 必要に応じてテストコードの実装、および `docs/test_specification.md` の更新を行う。

### Step 8.5: 人間による実装・テスト確認（必須）
1. ユーザーに対して、実装内容とテスト結果を詳細に報告し、確認を求める。
2. ユーザーから「ドキュメント更新へ進む」または「OK」の指示が出るまで、次のステップへ進んではならない。

### Step 9: 最終ドキュメントの反映【必須】
1. `docs/` ディレクトリ内を走査し、仕様書・設計書・テスト仕様書を今回の実装内容に合わせて最新化する。
> **注意**: `docs/changes/` 配下のファイルは履歴として残すため削除しないこと。

### Step 10: リポジトリへのPushとPR作成の提案
1. 以下のコマンドでリモートに作業ブランチをPushする。
   ```bash
   git push origin feature/{branch名}
   ```
2. 実装完了とテスト通過を確認後、エージェントは「PRの作成が可能です。以下の情報で作成しますか？」とタイトル・概要・変更履歴を添えて**ユーザーに Pull Request の作成を提案**する。

### Step 11: Pull Request の作成
1. ユーザーの承認後、[CONVENTIONS.md](./CONVENTIONS.md) のテンプレートに沿って GitHub CLI で Pull Request を作成する。
   ```powershell
   chcp 65001; gh pr create --title "{prefix}: {PRタイトル}" --body "Fixes #{Issue番号}`n`n### 変更履歴へのリンク`n- [changes](file:///docs/changes/{changesファイル名}.md)"
   ```

### Step 12: マージ後の整理とクローズ
1. `gh pr view` 等でマージ済み状態を確認する。
2. ローカルの `main` ブランチへ切り替えて最新化する。
   ```bash
   git checkout main
   git pull origin main
   ```
3. 役割を終えたローカルの作業ブランチをクリーンアップのため削除する。
   ```bash
   git branch -d feature/{branch名}
   ```
4. 紐付く GitHub Issue が自動でクローズされていない場合は、GitHub CLI を用いて手動でクローズし、完了報告を行う。
   ```powershell
   chcp 65001; gh issue close {Issue番号}
   ```

---

## ⚠️ エラーハンドリング・トラブル対応
エラーや中断時の対応ポリシーは **[ERROR_HANDLING.md](./ERROR_HANDLING.md)** に詳細が定義されています。何か問題が生じた場合は、その内容に従って適切に対処してください。
