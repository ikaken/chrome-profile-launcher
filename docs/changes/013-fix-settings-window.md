# #13 設定画面が開かない

## 背景
設定画面を開こうとすると、BAMLの型変換に関連する例外（System.Windows.Baml2006.TypeConverterMarkupExtension）が発生し、画面が表示されない不具合が報告されている。

## 目的
設定画面を正常に表示できるように修正する。

## 変更内容
- `SettingsWindow.xaml` の XAML 解析エラーを特定し修正する。
- 可能性のある原因：
    1. `Height`, `Width` 等のバインディング先が `double?` であることによる型変換失敗。
    2. `Icon` プロパティのパス指定ミス。

## 影響範囲
- 設定画面（SettingsWindow）

## 非対応
- 特になし

## 備考
- エラーメッセージ：`"System.Windows.Baml2006.TypeConverterMarkupExtension" の値の指定時に例外がスローされました。行番号 "10", 行位置 "9"。`
