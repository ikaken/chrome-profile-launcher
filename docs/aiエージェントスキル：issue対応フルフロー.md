# AIエージェントスキル：Issue対応フルフロー

---

## 目的

Issue対応において、設計・実装・ドキュメント・Git運用を一貫して実施し、品質とトレーサビリティを担保する。

---

## 基本原則

- mainブランチへ直接コミットしない
- 変更はIssue単位で管理する
- ドキュメントは常に最新状態を維持する
- 変更理由は必ず記録する

---

## フルフロー

### 1. Issue確認

- Issue内容を理解
- 不明点を洗い出す
- 必要ならIssueにコメントで補足

---

### 2. ブランチ作成

```bash
git checkout -b feature/{issue番号}-{内容}
```

例：

```bash
git checkout -b feature/012-profile-sort
```

---

### 3. ドキュメント初期化（必須）

- `docs/changes/` にファイル作成

```text
docs/changes/{番号}-{内容}.md
```

#### フォーマット

```md
# タイトル

## 背景
## 目的
## 変更内容
## 影響範囲
## 非対応
## 備考
```

---

### 4. 設計

- 実装方針を決定
- changesに記録
- 必要ならdesign.md更新（暫定可）

---

### 5. 実装

- コード変更
- 小さく分割して進める

---

### 6. コミット

```bash
git add .
git commit -m "feat: {内容}"
```

#### 規約

| prefix   | 用途     |
| -------- | ------ |
| feat     | 機能追加   |
| fix      | バグ修正   |
| refactor | リファクタ  |
| docs     | ドキュメント |

---

### 7. 実装中ドキュメント更新

- 設計変更はchangesに追記
- 重要判断は必ず記録

---

### 8. テスト

- 動作確認
- エッジケース確認
- 必要に応じてtest.md更新

---

### 9. 最終ドキュメント反映（必須）

- `spec.md` を最新仕様に更新
- `design.md` を実装内容に合わせる
- `test.md` を更新

※ changesは削除しない

---

### 10. Push

```bash
git push origin feature/{branch名}
```

---

### 11. Pull Request作成

#### 必須内容

- Issue紐付け
- changes参照
- 変更概要

```text
Fixes #{番号}
```

---

## ディレクトリ構成

```text
docs/
  ├ spec.md
  ├ design.md
  ├ test.md
  └ changes/
```

---

## 必須チェックリスト

-

---

## 禁止事項

- mainへ直接コミット
- changes未作成
- ドキュメント未更新
- Issue未紐付け

---

## 成果

- 変更履歴が完全に追跡可能
- ドキュメントと実装が一致
- 安定した開発フローを維持

