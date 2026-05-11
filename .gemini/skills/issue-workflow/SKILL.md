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
10. **[Generalist]** Step 10-11: Push および Pull Request 作成。

---

## ⚠️ エラー・フォールバック
- クォータエラー発生時は、Architectから状況を報告し、Generalist（軽量モデル）への切り替えをユーザーに提案してください。
- 詳細は [ARCHITECT.md](./ARCHITECT.md) を参照。
