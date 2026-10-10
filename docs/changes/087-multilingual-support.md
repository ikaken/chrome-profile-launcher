# 英語と日本語以外の言語にも対応

Issue: https://github.com/ikaken/chrome-profile-launcher/issues/87

## 背景 / 目的

現在の対応言語は日本語（ニュートラル `Resources.resx`）と英語（`Resources.en.resx`）の2言語のみ。
resx サテライトアセンブリ方式を採用しているため、言語ごとの resx 追加で拡張可能だが、
以下の点が多言語化を阻害している。

- 設定画面の言語プルダウンが `ja-JP` / `en-US` の2項目ハードコード
- 初回起動の言語判定 `App.GetInitialLanguage()` が ja/en の2分岐
- ニュートラル resx が日本語のため、部分翻訳・未対応言語フォールバック時に日本語が表示される
- UI の MessageBox 等にハードコードされた日本語が残存

## 変更内容

### 1. ニュートラル言語を英語に変更（フォールバック改善）

- `Resources.resx`（ニュートラル）の値を英語に差し替え（現 `Resources.en.resx` の内容を採用）
- `Resources.ja.resx` を新規作成し、現ニュートラルの日本語を移動
- `Resources.en.resx` は英語がニュートラルになるため削除
- 効果: 未対応言語 OS・部分翻訳の未翻訳キーのフォールバック先が英語になる

### 2. 対応言語の一元管理

- `Helpers/SupportedLanguages.cs` を新規作成
  - `LanguageOption { CultureCode, DisplayName }`（DisplayName は `CultureInfo.NativeName`）
  - `Items`: 対応言語一覧（`en`, `ja`, + 追加言語）
  - `ResolveSupportedLanguage(CultureInfo)`: OS カルチャ → 対応言語への解決
    （完全一致 → 親カルチャ一致 → フォールバック `en`）

### 3. 初回言語判定の一般化

- `App.GetInitialLanguage()` を `LocalizationManager` / `SupportedLanguages` の解決ロジックに置き換え
  （例: `es-MX` OS → `es` リソースがあれば `es`、なければ `en`）

### 4. 設定画面の言語プルダウン動的生成

- `SettingsViewModel` に `AvailableLanguages` プロパティを追加
- `SettingsWindow.xaml` の ComboBox を `ItemsSource` バインド化
  （`DisplayMemberPath=DisplayName`、`SelectedValuePath=CultureCode`）
- 言語名リソース（`LanguageJapanese` 等）の個別追加が今後不要になる

### 5. 新言語 resx の追加

- 初回バッチ: **zh-CN / zh-TW / ko-KR / fr / de / es** の6言語（全42キー、機械翻訳ベース）
- `Resources.<culture>.resx` を対応言語分追加
- csproj に `DependentUpon` メタデータを追加（Solution Explorer の入れ子表示用）

### 6. 残存ハードコード文字列のリソース化

- `SettingsViewModel.cs`: 「現在、最新のバージョンを使用しています。」「アップデートの確認」「Folder not found」「Error opening folder」
- `MainViewModel.cs`: 「アップデートのダウンロードに失敗しました。」
- 上記を新規リソースキー化（format 文字列対応）

### 7. テスト

- `ResolveSupportedLanguage` の単体テスト（完全一致 / 親カルチャ / フォールバック）
- 全対応言語について全キーが非空であることのリソーステスト

## 影響範囲

- `Helpers/SupportedLanguages.cs`（新規）、`Helpers/LocalizationManager.cs`、`App.xaml.cs`
- `ViewModels/SettingsViewModel.cs`、`ViewModels/MainViewModel.cs`、`Views/SettingsWindow.xaml`
- `Properties/Resources*.resx`（追加・差替・削除）、`ChromeProfileLauncher.csproj`
- テストプロジェクト
- ユーザーへの影響:
  - 既存ユーザーの `settings.Language`（ja-JP/en-US）はそのまま有効
  - 新規インストール時、OS 言語が対応言語なら初期言語がその言語になる
  - 設定画面から新言語を選択可能。切替は従来どおり即時反映

## 備考

- 「自動 (システム設定)」選択肢（`LanguageAuto` キーは既存）は本 Issue では追加しない
- 機械翻訳ベースのため翻訳品質にばらつきあり。翻訳修正は言語単位で後続 Issue 対応可能
- `SettingsService.cs` の保存失敗 MessageBox など深い層の文字列は本 Issue 対象外
- 初回設定画面への言語選択追加は Issue #86 で見送り済みのため対象外
