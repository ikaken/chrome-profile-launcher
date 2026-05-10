# 変更履歴 016: 自動アップデート機能の導入

## 概要
ユーザーが常に最新版を利用できるよう、Velopack を利用したオンライン自動アップデート機能を導入しました。

## 変更内容
- **サービス層**: `UpdateService` を追加。GitHub Releases API を介して更新を確認・取得・適用。
- **UI層**: `MainViewModel` にアップデートチェック・通知ロジックを追加。
- **初期化**: `App.xaml.cs` で Velopack のライフサイクル管理を有効化。
- **CI/CD**: `.github/workflows/release.yml` を作成。Gitタグのプッシュでインストーラーとパッチを自動生成。
- **ドキュメント**: `docs/release_manual.md` を作成。

## 影響範囲
- アプリ起動時にインターネット接続が発生します（アップデートチェックのため）。
- 従来の `setup.iss` (Inno Setup) によるインストーラー作成は、今後は GitHub Actions による自動生成に移行することを推奨します。

## テスト状況
- コードのコンパイル確認済み（`dotnet build`）。
- Velopack パッケージの参照整合性を確認。
