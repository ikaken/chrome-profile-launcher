# リリース作業マニュアル

このドキュメントでは、Chrome Profile Launcher の新バージョンをリリース（公開）する際の手順について説明します。
自動アップデート機能（Velopack）と GitHub Actions の導入により、リリースの大部分は自動化されています。

## リリースフローの全体像

```
feature/xxx → develop (PR) → Pre-releaseタグ (v1.1.0-beta) → テスト
                                    ↓ 問題なければ
main (PR・マージ) → 正式版タグ (v1.1.0) → 自動アップデート
```

| タグ | ベースブランチ | リリース種別 | 自動アップデート |
|---|---|---|---|
| `v1.1.0-beta` | `develop` | Pre-release（手動DL） | 対象外 |
| `v1.1.0` | `main` | Latest release | 対象 |

## バージョン番号のルール

プロジェクトは `v{major}.{minor}.{patch}` 形式を採用する。

| 位置 | 変更条件 |
|---|---|
| **major** | ユーザーが明示的に指示した時のみ |
| **minor** | 機能追加（`enhancement` タグの Issue 対応時）。minor を上げたら patch を `0` にリセット |
| **patch** | バグ修正（`bug` タグの Issue 対応時） |

**決定手順（省略禁止）:**
1. `git tag --sort=-version:refname` で現在の最新タグを確認
2. `gh issue view <番号> --json labels` で Issue のラベルを確認
3. ラベルに応じてバージョンを計算

## Pre-release 手順（develop ブランチから）

### 1. 事前確認
```powershell
git checkout develop
git pull origin develop
git status  # クリーンであること
```

### 2. タグ作成・プッシュ（サフィックスに `-beta` を必ず付ける）
```powershell
git tag v1.1.0-beta
git push origin v1.1.0-beta
```

### 3. Actions 監視
GitHub の **[Actions]** タブで `Release` ワークフローの完了を確認する。
`-beta` タグは自動的に **Pre-release** として公開される。

### 4. リリースノート編集
GitHub の **[Releases]** から該当リリースを編集して公開する。

## 正式版リリース手順（main ブランチから）

### 1. 事前確認
`develop` → `main` の PR をマージ後:
```powershell
git checkout main
git pull origin main
git status  # クリーンであること
```

### 2. タグ作成・プッシュ（サフィックスなし）
```powershell
git tag v1.1.0
git push origin v1.1.0
```

### 3. Actions 監視
**Latest release** として公開されることを確認する。

### 4. README 更新
ダウンロードリンクとバージョン表記を新しいタグに更新してコミット・プッシュする。

### 5. リリースノート編集
GitHub の **[Releases]** から該当リリースを編集して公開する。

### 6. 自動アップデートの確認
- **通知タイミング**: アプリ起動の 3 秒後にチェックが行われる
- アプリを起動し「新しいバージョンが利用可能です」ダイアログが表示されるか確認する

---

## ⚠️ 注意事項
- **タグの重複禁止**: 一度プッシュしたタグは削除・再利用しない。Velopack の整合性が壊れる可能性がある。
- **ビルドエラー時**: Actions のログを確認し、問題を修正の上、新しいバージョン番号で再リリースする。
- **Issue クローズのタイミング**: Pre-release テスト中はクローズせず、`main` マージ後にクローズする。
