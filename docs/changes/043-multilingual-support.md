# Issue #43 アプリの多言語対応

## 概要
アプリ内のUI文字列およびメッセージを多言語（日本語・英語）に対応させ、設定画面から言語を切り替えられるようにします。

## 変更内容
- [ ] 多言語リソースファイルの作成（Resources.resx, Resources.en.resx）
- [ ] XAMLファイルのローカライズ（StaticResource / DynamicResource / Binding への置き換え）
- [ ] ViewModel内のメッセージ文字列のローカライズ
- [ ] 設定画面に言語選択オプションを追加
- [ ] `SettingsService` に言語設定の保存機能を追加
- [ ] 言語切り替え時のリアルタイム反映

## 影響範囲
- `MainWindow.xaml`
- `SettingsWindow.xaml`
- `MainViewModel.cs`
- `SettingsViewModel.cs`
- `SettingsService.cs`
- `Models/ProfileInfo.cs` (Settings)

## テスト項目
- [ ] 初期起動時にOSの言語設定が反映されること
- [ ] 設定画面で言語を切り替え、保存するとUIが即座に更新されること
- [ ] アプリ再起動後も選択した言語が保持されていること
