# Microsoft Store 公開ガイド

Chrome Profile Launcher を Microsoft Store に公開するための手順書です。
対象読者: リポジトリ管理者（人間）。MSIX の生成自体は GitHub Actions が自動で行います。

## 1. 概要

### なぜ Microsoft Store か

- Windows 11 22H2 以降の「スマート アプリ コントロール（SAC）」は、署名のないアプリを**回避手段なしにブロック**します。
- Store 経由で配布される MSIX パッケージは **Microsoft が署名**するため、SAC / SmartScreen の警告なしにインストールできます。
- 更新は Store が自動で行うため、Velopack による独自更新も不要になります（Store 版ではアプリ内の自動更新は自動的に無効化されます）。

### 費用

- 2025 年 9 月より、**個人開発者アカウントの登録料は無料**です（要: 政府発行 ID による本人確認）。

### 配布チャネルの整理

| チャネル | 形式 | 署名 | 対象 |
|---|---|---|---|
| GitHub Releases（既存） | Velopack Setup.exe | なし | SAC が無効 / 評価モードの PC |
| Microsoft Store（本ガイド） | MSIX | Microsoft | 全ユーザー（推奨） |

## 2. Partner Center 個人開発者アカウントの作成

- [ ] https://storedeveloper.microsoft.com にアクセスする
- [ ] 「Get started for free」をクリックする
- [ ] 「Individual developer」（個人開発者）を選択する
- [ ] Microsoft アカウントでサインインする（**MFA（多要素認証）が必須**。未設定の場合は先に設定する）
- [ ] 本人確認を行う: 政府発行 ID（運転免許証・マイナンバーカード・パスポート等）の撮影 + セルフィー撮影。**スマートフォンでの実施が推奨**（案内に従い QR コード等でスマホに移行する）
- [ ] 開発者プロフィール（表示名・連絡先など）を確認・入力する
- [ ] Partner Center のダッシュボードにアクセスできることを確認する

## 3. アプリ名の予約

※ Partner Center の UI は言語設定により日本語 / 英語で表示されます。以下は「日本語表記（英語表記）」で記載します。

- [ ] Partner Center で「アプリとゲーム（Apps and games）」→「＋ 新しい製品（New product）」を選択する
- [ ] アプリの種類として「MSIX または PWA アプリ（MSIX or PWA app）」を選択する（ゲーム向けの「GDK Game」ではない）
- [ ] アプリ名に **「Chrome Profile Launcher」** と入力し、「製品名の予約（Reserve product name）」で予約する
  - ※ 既に他者に予約されている場合は別名を検討し、`Packaging/AppxManifest.xml` の DisplayName 等も合わせて変更する

## 4. Identity 値の取得

予約した製品のページから MSIX マニフェストに必要な値を取得します。

- [ ] Partner Center で製品を開き、「製品管理（Product management）」→「製品 ID（Product identity）」ページを表示する
- [ ] 以下の値を控える:
  - `Package/Identity/Name` … 例: `12345Publisher.ChromeProfileLauncher` 形式
  - `Package/Identity/Publisher` … `CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`（GUID 形式）
  - `Package/Properties/PublisherDisplayName` … ストアに表示される発行者名

## 5. GitHub リポジトリ変数の設定

リリースワークフローがマニフェストのプレースホルダを置換するために使用します。

- [ ] GitHub リポジトリの「Settings」→「Secrets and variables」→「Actions」→「Variables」タブを開く
- [ ] 以下のリポジトリ変数を作成する:

| 変数名 | 値 |
|---|---|
| `MSIX_IDENTITY_NAME` | `Package/Identity/Name` の値 |
| `MSIX_PUBLISHER` | `Package/Identity/Publisher` の値（`CN=...`） |
| `MSIX_PUBLISHER_DISPLAY_NAME` | `Package/Properties/PublisherDisplayName` の値 |

- [ ] 未設定の場合、ワークフローの MSIX ステップは自動的にスキップされます（Velopack リリースには影響しません）

## 6. リリースと MSIX の取得

- [ ] 通常どおり `vX.Y.Z` 形式のタグを作成して push する（例: `git tag v1.1.0 && git push origin v1.1.0`）
  - ※ `-beta` / `-rc` を含むプレリリースタグでは MSIX は生成されません
- [ ] GitHub Actions の「Release」ワークフローが完了するのを待つ
- [ ] ワークフロー実行ページの Artifacts から `ChromeProfileLauncher-msix-X.Y.Z` をダウンロードし、zip を展開して `.msix` ファイルを取り出す
  - ※ この .msix は**未署名**です。Partner Center に提出すれば Microsoft が署名するため、ローカルで署名する必要はありません

## 7. Partner Center への提出

- [ ] Partner Center で製品を開き、「Start your submission」をクリックする
- [ ] 「Packages」: ダウンロードした `.msix` をアップロードする
- [ ] 「Pricing and availability」: 価格を「Free」（無料）にし、配布マーケットは全マーケットを選択する
- [ ] 「Properties」: カテゴリを設定する（例: Productivity / Utilities & tools）、システム要件（Windows 10 バージョン 17763 以降、x64）を確認する
- [ ] 「Age ratings」: IARC のアンケートに回答し、年齢レーティングを取得する
- [ ] 「Store listings」: 少なくとも英語（en-US）のリスティングを作成する
  - 説明文（日本語リスティングも追加可能）
  - **スクリーンショット 1 枚以上**（推奨: 1366x768 以上）
  - **プライバシーポリシー URL**: 本アプリはネットワークへ個人情報を送信しない旨を記載したページの URL を設定する（例: GitHub リポジトリ内の docs ページ）
- [ ] 「Submission options」: 公開タイミング等を確認する
- [ ] 「Submit」をクリックする
- [ ] 審査結果を待つ（通常 1〜3 営業日）

## 8. 公開後の作業

- [ ] Store の製品ページ URL を確認し、`README.md` の `TODO: Store 公開後に URL を設定` コメントを実際のリンクに置き換える
  - アプリ内リンク: `ms-windows-store://pdp/?productid=XXXXXXXXXXXX`
  - Web リンク: `https://apps.microsoft.com/detail/XXXXXXXXXXXX`
- [ ] （任意）GitHub リポジトリに Store バッジを追加する

## 9. 以後のバージョンアップ手順

- [ ] `vX.Y.Z` タグを push する（Store の MSIX バージョンは自動的に `X.Y.Z.0` になる）
- [ ] Actions の artifact から新しい `.msix` をダウンロードする
- [ ] Partner Center で製品を開き「Update」→ 新しい提出を作成する
- [ ] 「Packages」で古いパッケージを新しい `.msix` に置き換える
- [ ] 必要に応じて Store listings（更新内容の説明など）を更新する
- [ ] 「Submit」で再審査に提出する

## 10. ローカル検証（任意・開発者向け）

提出前に MSIX の動作をローカルで確認する場合の手順です。
前提: Windows SDK（makeappx.exe, signtool.exe）がインストールされていること。

### 10.1 MSIX のローカル生成

```powershell
# publish
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishReadyToRun=true /p:Version=0.0.1

# ステージング
$publishDir = "./bin/Release/net10.0-windows10.0.17763.0/win-x64/publish"
Copy-Item -Recurse $publishDir ./msix-staging
Copy-Item -Recurse ./Packaging/Assets ./msix-staging/Assets

# マニフェストのプレースホルダをダミー値で置換
$manifest = Get-Content ./Packaging/AppxManifest.xml -Raw -Encoding UTF8
$manifest = $manifest.Replace('{{IDENTITY_NAME}}', 'Dummy.ChromeProfileLauncher')
$manifest = $manifest.Replace('{{PUBLISHER}}', 'CN=00000000-0000-0000-0000-000000000000')
$manifest = $manifest.Replace('{{PUBLISHER_DISPLAY_NAME}}', 'Dummy Publisher')
$manifest = $manifest.Replace('{{VERSION}}', '0.0.1.0')
Set-Content ./msix-staging/AppxManifest.xml -Value $manifest -Encoding UTF8

# MSIX 生成
$makeappx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.*\x64\makeappx.exe" |
  Sort-Object FullName -Descending | Select-Object -First 1
& $makeappx.FullName pack /d ./msix-staging /p ./msix/ChromeProfileLauncher-0.0.1.0.msix /o
```

### 10.2 自己署名証明書でのテスト署名

未署名の MSIX はインストールできないため、テスト用に自己署名します。

```powershell
# 証明書作成（Subject はマニフェストの Publisher と同一にする）
$cert = New-SelfSignedCertificate -Type Custom `
  -Subject "CN=00000000-0000-0000-0000-000000000000" `
  -KeyUsage DigitalSignature `
  -FriendlyName "ChromeProfileLauncher Test" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# pfx エクスポート
$pw = ConvertTo-SecureString -String "password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ./test-cert.pfx -Password $pw

# 署名
$signtool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.*\x64\signtool.exe" |
  Sort-Object FullName -Descending | Select-Object -First 1
& $signtool.FullName sign /fd SHA256 /a /f ./test-cert.pfx /p password ./msix/ChromeProfileLauncher-0.0.1.0.msix

# 証明書を「信頼されたユーザー」にインポート
Import-Certificate -FilePath (Export-Certificate -Cert $cert -FilePath ./test-cert.cer -Force -PassThru) -CertStoreLocation Cert:\CurrentUser\TrustedPeople

# インストール
Add-AppxPackage ./msix/ChromeProfileLauncher-0.0.1.0.msix
```

### 10.3 確認観点

- [ ] アプリが起動し、**初回セットアップ画面が表示される**（設定ファイルが無いため）
- [ ] 設定画面を開き、**自動更新の行が非表示**になっている
- [ ] 設定で「Windows 起動時に実行」を ON にすると、タスク マネージャーの「スタートアップ アプリ」に Chrome Profile Launcher が登録される
- [ ] 手動更新チェックを実行してもエラーにならない（ログに "Updates are handled by Microsoft Store." と出力される）

### 10.4 アンインストールと後片付け

```powershell
Get-AppxPackage *ChromeProfileLauncher* | Remove-AppxPackage
Remove-Item Cert:\CurrentUser\My\$($cert.Thumbprint)
Remove-Item Cert:\CurrentUser\TrustedPeople\$($cert.Thumbprint)
```

## 11. トラブルシューティング

### 提出時に Identity 関連のエラーが出る

- `Package/Identity/Name`・`Package/Identity/Publisher` が Partner Center の値と完全に一致しているか確認する（`MSIX_IDENTITY_NAME` / `MSIX_PUBLISHER` のコピー漏れ・余分な空白に注意）
- Publisher は `CN=` から始まる GUID 形式の文字列そのままを設定する

### バージョンエラー（4 桁目が 0 でない）

- Store の MSIX バージョンは `X.Y.Z.0` 形式で 4 桁目が 0 である必要があります。ワークフローが自動で `X.Y.Z.0` に変換するため、タグは `vX.Y.Z` 形式を維持してください

### runFullTrust に関する審査の注記

- 本アプリはフルトラスト（デスクトップブリッジ）アプリのため `runFullTrust` ケイパビリティを宣言しています。審査時に用途の説明を求められる場合は「デスクトップアプリとして Chrome プロファイルを起動するため」と回答してください

### MSIX ステップが実行されない

- リポジトリ変数 `MSIX_IDENTITY_NAME` が設定されているか確認する（未設定だとスキップされます）
- タグ名に `-beta` / `-rc` が含まれていないか確認する
