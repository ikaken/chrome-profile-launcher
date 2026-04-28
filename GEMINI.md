# Chrome Profile Launcher - Gemini CLI Project Context

このファイルは Gemini CLI 用のプロジェクト固有のガイドラインを定義します。

## 🛠 開発ガイドライン (WPF/C#)

### リソースとパス解決
- **Icon指定**: XAML 内で `Icon` を指定する際は、サブフォルダからのパス解決失敗を防ぐため、必ず **pack URI** 形式を使用してください。
  - 推奨: `Icon="pack://application:,,,/ChromeProfileLauncher;component/Assets/app.ico"`
- **バインディング**: ウィンドウの座標やサイズ（`Top`, `Left`, `Width`, `Height`）にバインディングするプロパティは、`double?` ではなく **`double`** 型を使用してください。初期化時の `null` 値による `XamlParseException` を防止するためです。

## 📋 ワークフロー
- **Issue対応**: `.gemini/skills/issue-workflow/` に定義されたスキルに従ってください。
- **ドキュメント更新**: 実装変更時は必ず `docs/changes/` に履歴を残し、関連する仕様書を同期させてください。

## ⚠️ 注意事項
- `.antigravity` フォルダは既存のツールで使用されているため、変更や削除を行わないでください。
- ビルド時はアプリが起動していないことを確認してください（ファイルロック回避のため）。
