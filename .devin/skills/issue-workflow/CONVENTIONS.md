# Issue Handling: Conventions & Templates

本プロジェクトで Issue 対応を行う際の共通規約です。全てのエージェントはこの形式を厳守してください。

## ブランチ命名規則
- **形式**: `feature/{Issue番号}-{内容の英単語}`
- **例**: `feature/012-profile-sorting`

## 履歴ドキュメント (`docs/changes/`)
- **ファイル名**: `{Issue番号}-{内容の英単語}.md`
- **テンプレート**:
  ```markdown
  # {Issueタイトル}

  ## 背景 / 目的
  {なぜこの変更が必要なのか、何を解決するのか}

  ## 変更内容
  - {変更点1}
  - {変更点2}

  ## 影響範囲
  - {修正したクラス/メソッド}
  - {ユーザーへの影響}

  ## 備考
  {設計上の判断、トレードオフ、未対応事項など}
  ```

## コミットメッセージ規約
- **形式**: `{prefix}: {日本語での変更内容}`
- **Prefix一覧**:
  - `feat`: 新機能の追加
  - `fix`: バグ修正
  - `refactor`: コードの整理（挙動は変わらない）
  - `docs`: ドキュメントのみの変更
  - `test`: テストコードの追加・修正
- **例**: `feat: プロファイル表示のソートロジックを追加`

## Pull Request 概要
- **必須項目**:
  - `Fixes #{Issue番号}`
  - `docs/changes/{ファイル名}.md` へのリンク
  - 動作確認済みのエビデンス（テスト結果の要約）

## 推奨コマンド
- **Git**: `git checkout -b ...`, `git commit -m "..."`, `git push origin ...`
- **GitHub CLI (Windows)**:
  - Issue一覧取得（UTF-8対策済み）: `powershell -Command "$OutputEncoding = [System.Text.Encoding]::UTF8; gh issue list -s open --limit 20"`
  - Issue詳細取得: `chcp 65001; gh issue view {number}`
  - PR作成: `chcp 65001; gh pr create --title "..." --body "..."`
- **検証**: `dotnet test` 等、プロジェクトに応じたテストコマンド。
