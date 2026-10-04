# プロファイルのリロードでプロファイル名の変更が反映されない

Issue: https://github.com/ikaken/chrome-profile-launcher/issues/81

## 背景 / 目的
Chrome 側でプロファイル名を変更した後、設定画面の「プロファイルのリロード」ボタンを押しても、
変更後のプロファイル名がアプリに反映されない。

### 根本原因
`SettingsViewModel.ReloadProfilesCommand` および `MainViewModel.LoadProfilesAsync` のマージ処理では、
既存設定に存在するプロファイル（Id が一致するもの）について `IconPath` のみを検出結果から上書きし、
`DisplayName` は既存設定の値をそのまま保持している。そのため `settings.json` に保存された古い名前が
永続的に使われ続け、Chrome 側の名前変更がいつまでも反映されない。

仕様書（`docs/chromeプロファイルランチャ仕様書（wpf最終版）.md`）では
「プロファイル名およびアイコンの編集機能は提供しない」「プロファイル名は読み取り専用」と定めており、
アプリ内で独自の表示名を持つ機能は存在しない。したがって `DisplayName` は Chrome の `Local State` を
正とすべきであり、既存設定の値を優先する現在の挙動は仕様に反している。

## 変更内容
- `ViewModels/SettingsViewModel.cs` `ReloadProfilesCommand`:
  既存プロファイルのマージ時に `IconPath` に加えて `DisplayName` も検出結果で上書きする。
- テスト更新:
  - `SettingsViewModelTests.ReloadProfilesCommand_ShouldMergeDetectedProfiles`:
    既存プロファイルの `DisplayName` が検出結果の名前（`"Default P1"`）に更新されることを検証するよう修正。
    `IsVisible` / `Order` は既存設定の値が保持されることを併せて検証する。
- `MainViewModel.LoadProfilesAsync`（起動時ロード）は**変更しない**。
  起動時の処理を最小限に保つ方針のため、名前の更新は「プロファイルのリロード」操作時のみ行う。

## 影響範囲
- `SettingsViewModel.ReloadProfilesCommand`
- ユーザーへの影響:
  - Chrome でプロファイル名を変更した場合、設定画面の「プロファイルのリロード」ボタン押下時に新しい名前が反映される。
    設定を保存すれば次回起動以降も新しい名前が表示される。
  - 起動時の挙動は変わらない（起動時には自動反映されない）。
  - 表示/非表示・並び順の設定は従来通り保持される。
  - 副次効果として、`LauncherService` のウィンドウタイトルマッチ（`DisplayName` を使用）が
    最新の名前で行われるようになり、ウィンドウ特定の精度が改善する。

## 備考
- 既存テストのコメント「Should preserve custom name」は、実装されていない「独自表示名」機能を
  前提としたものであり、仕様（名前は読み取り専用）と矛盾していたため、仕様に合わせて修正する。
- `MainViewModelTests.LoadProfiles_ShouldMergeSettingsAndDiscovery` は起動時に既存名を保持する
  現行挙動を検証しており、今回は変更対象外のため維持する。
- 将来アプリ内で表示名を編集する機能を追加する場合は、`ProfileInfo` に
  「ユーザー上書き名」を別プロパティとして持たせる設計が必要になる（本 Issue のスコープ外）。
