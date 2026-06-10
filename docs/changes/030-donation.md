# Issue #30: ドネーション対応

## 概要
アプリの継続的な開発を支援するためのドネーション（寄付）機能を実装しました。設定画面の「アプリ設定」タブに、GitHub Sponsors をメイン、Ko-fi をサブとした支援リンクを含むカードを追加しました。

## 変更内容

### ViewModel (`ViewModels/SettingsViewModel.cs`)
- `OpenUrlCommand` を追加
  - 指定された URL をデフォルトのブラウザで開く機能。
  - Windows 環境での適切なプロセス起動（`UseShellExecute = true`）を考慮。

### UI (`Views/SettingsWindow.xaml`)
- 「アプリ設定」タブの最下部にドネーションカードを追加。
  - ☕ アイコンと支援のお願いを記載したテキスト。
  - GitHub Sponsors のメインリンクと、Ko-fi のサブリンクをシンプルに配置。
  - GitHub Sponsors では月額支援と一回払いの両方を想定。
  - 既存のカードUI（Border, CornerRadius, Background）とデザインを統一。

## 影響範囲
- 設定画面（SettingsWindow）のみ。既存の機能への影響はありません。

## 検証内容
- [ ] 設定画面の「アプリ設定」タブにドネーションカードが表示されること。
- [ ] 各ボタンをクリックした際、対応する URL がブラウザで開くこと。
- [ ] 既存の設定項目の操作に支障がないこと。
- [ ] GitHub Sponsors をメイン、Ko-fi をサブとした支援構成が明確であること。
