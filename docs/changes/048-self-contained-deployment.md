# Issue #48: .NETを別途インストールせずに利用できる配布方式へ変更

## 背景 / 目的
現在のフレームワーク依存ビルドでは、.NET Desktop Runtimeが未導入の環境で利用者に追加インストールを要求する。必要な.NETランタイムを配布物へ同梱し、追加操作なしでアプリを起動できるようにする。

## 変更内容
- `ChromeProfileLauncher.csproj` の `SelfContained` を `true` に変更し、Windows x64向けランタイムを同梱する。
- GitHub Actionsの `dotnet publish` を `--self-contained true` に変更し、設定がコマンドラインでも明示されるようにする。
- Velopack SDKとCLIを、復元可能な同一バージョン `0.0.1251` に固定してパッケージ互換性を保つ。
- Velopackにはpublishディレクトリ全体を渡し、アプリ本体と.NETランタイムを同一パッケージへ格納する。
- READMEおよびリリース関連文書のpublishコマンドと配布方式の説明をself-containedへ更新する。
- `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishReadyToRun=true` を実行し、ランタイムを含む成果物が生成されることを確認する。
- 既存のユニットテストとReleaseビルドを実行し、配布方式変更による回帰がないことを確認する。

## 影響範囲
- `ChromeProfileLauncher.csproj`: 既定のpublish方式。
- `.github/workflows/release.yml`: リリース成果物の生成方式。
- `installer/setup.iss`: 旧手動インストーラーを使用する場合もランタイムを含むpublish成果物全体を格納する。
- `README.md`, `docs/design_document.md`, `docs/release_guide.md`: ビルド・配布手順と設計説明。
- GitHub Releaseに添付されるVelopackパッケージおよびインストーラーのファイルサイズが増加する。
- 対象環境は従来どおりWindows x64とし、アプリ機能および設定形式は変更しない。

## 備考
- self-contained配布では利用者PCの.NETランタイムに依存しない一方、.NETのセキュリティ更新はアプリを再ビルド・再配布して反映する。
- `PublishSingleFile` は導入せず、Velopackがpublishディレクトリ内の複数ファイルをまとめる既存構成を維持する。
- Velopackの.NETブートストラップ機能ではなくself-containedを採用し、インストール時の外部ダウンロードを不要にする。
- 現在のリリースワークフローは過去パッケージを取得していないため、アップデートは毎回フルパッケージをダウンロードする。self-contained化により通信量が増えるため、差分更新の導入を別途検討する。
- WPFはアセンブリトリミング非対応のため、サイズ削減目的の `PublishTrimmed` は導入しない。
