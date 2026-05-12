---
name: issue-workflow
description: GitHubのIssue対応を自動化するマルチエージェント・ワークフロー。ArchitectとGeneralistが連携し、高品質な開発を実現します。
---

# Issue Handling Workflow (Core)

## 🎯 目的
高度な推論を必要とする設計・実装（Architect）と、定型的なGit操作・検証（Generalist）を組み合わせた、GitHub Issue対応の全体プロセスを定義します。

## 🤖 エージェント構成
| エージェント | 役割 | モデル | ガイドライン |
| :--- | :--- | :--- | :--- |
| **Architect** | 分析・設計・高度な実装・仕様同期 | gemini-3-flash-preview | [ARCHITECT.md](./ARCHITECT.md) |
| **Generalist** | 定型作業・Git操作・テスト・記録 | gemini-3.1-flash-lite-preview | [GENERALIST.md](./GENERALIST.md) |

## 🛑 共通規約
全ての作業は [CONVENTIONS.md](./CONVENTIONS.md) に定義された命名規則とテンプレートに従ってください。

---

## 🔄 全体フロー

### Phase 1: 初期化
1. **[Generalist]** Step 0: 環境チェックを実行。
2. **[Architect]** Step 1: Issue分析と要件定義。

## 🚀 ワークフローの実行条件
以下のいずれかの指示がトリガーとなり、フローが開始されます。

- **「イシューの一覧を取得してください」**: 進行中の作業状況を把握し、次の着手対象を決定するための開始シグナルです。
- **「Issue #xx に着手してください」**: 特定のイシューに対する具体的な開発・修正作業を開始するシグナルです。
- **「Issue #xx の調査と修正をお願いします」**: 調査から実装・ドキュメント同期までをフルフローで回すシグナルです。

### Phase 2: 作業準備
3. **[Generalist]** Step 2: 作業用ブランチ作成。
4. **[Generalist]** Step 3: `docs/changes/` ドキュメントの初期化。

### Phase 3: 設計とレビュー
5. **[Architect]** Step 4: 設計・方針策定。
6. **[User]** Step 4.5: **設計レビュー（承認必須）**

### Phase 4: 実装と検証
7. **[Architect & Generalist]** Step 5-8: 実装、コミット、テストの反復。
   - Architectが実装し、Generalistがコミットとテストを担当する。
8. **[User]** Step 8.5: **実装・動作確認（承認必須）**

### Phase 5: 完了
9. **[Architect]** Step 9: プロジェクト主要ドキュメントへの反映。
10. **[Generalist]** Step 10: **「PR作成の提案」を行う**
    - 実装完了とテスト通過を確認後、エージェントは自動的に「PRの作成が可能です。以下の情報で作成しますか？」とタイトル・概要・変更履歴を添えてユーザーに提案する。
11. **[Generalist]** Step 11: **Push および Pull Request 作成**
    - ユーザーの承認後、GitHub CLI (`gh pr create`) を実行する。
12. [Generalist] Step 12: **マージ後の整理とクローズ**
    - ユーザーからPull Requestのマージ完了報告を受ける。
    - エージェントは GitHub CLI (`gh pr view`) 等でマージ状態を確認する。
    - `git checkout main && git pull origin main` を実行し、ローカル環境を最新のメインラインに同期する。
    - 不要となった作業用ブランチを削除する（任意）。
    - 関連する GitHub Issue を `gh issue close <number>` で自動的にクローズし、その旨を報告する。

---

## ⚠️ エラー・フォールバック
- クォータエラー発生時は、Architectから状況を報告し、Generalist（軽量モデル）への切り替えをユーザーに提案してください。
- 詳細は [ARCHITECT.md](./ARCHITECT.md) を参照。
