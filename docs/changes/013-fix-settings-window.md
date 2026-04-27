# #13 設定画面が開かない

## 背景
設定画面を開こうとすると、BAMLの型変換に関連する例外（System.Windows.Baml2006.TypeConverterMarkupExtension）が発生し、画面が表示されない不具合が報告されている。

## 目的
設定画面を正常に表示できるように修正する。

## 変更内容
- `Views/SettingsWindow.xaml` の `Icon` パスを `Assets/app.ico` から `/Assets/app.ico` に変更。
- `MainWindow.xaml` の `Icon` パスも `/Assets/app.ico` に統一。

## 修正結果
- サブフォルダ（`Views/`）内にある XAML ファイルからも正しくリソースが参照されるようになり、BAML 解析時の `TypeConverterMarkupExtension` 例外が解消された。
- ローカル環境でのビルドが正常に完了することを確認。

## 影響範囲
- 設定画面（SettingsWindow）
- メイン画面（MainWindow）

## 非対応
- 特になし

## 備考
- エラーの原因は、XAML ファイルの階層構造に起因する相対パス解決の失敗であった。
