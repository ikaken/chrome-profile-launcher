# ドネーション（開発支援）設定手順書

このドキュメントでは、Chrome Profile Launcher のドネーション機能で使用する **GitHub Sponsors** と **PayPal** の設定方法を解説します。

---

## 1. GitHub Sponsors の設定方法

GitHub Sponsors は、手数料が無料でエンジニア文化に馴染みやすいため、最も推奨される方法です。

### ステップ 1: プログラムへの参加
1. [GitHub Sponsors](https://github.com/sponsors) にアクセスします。
2. 「Get sponsored」をクリックし、自身のアカウントを選択します。
3. 支払先情報（Stripe アカウントの作成が必要）やプロフィールの設定を完了させます。
   - ※ 審査に数日かかる場合があります。

### ステップ 2: 支援ティア（金額）の設定
1. 管理画面の「Sponsorship tiers」から「Add a tier」をクリックします。
2. 「One-time payment（単発）」を選択します。
3. アプリの実装に合わせて以下の金額を作成するとスムーズです。
   - **300円**: 「コーヒー1杯」
   - **500円**: 「軽く応援」
   - **1000円**: 「開発支援」

### ステップ 3: リンクの取得とアプリへの反映
- あなたの Sponsors ページの URL は `https://github.com/sponsors/YOUR_GITHUB_ID` になります。
- 特定の金額を指定して開く場合は、以下の形式をアプリの `SettingsWindow.xaml` に設定します。
  - `https://github.com/sponsors/YOUR_GITHUB_ID?frequency=one-time&amount=300`

---

## 2. PayPal の設定方法

GitHub アカウントを持っていないユーザー向けに、PayPal での支援も受け付けることができます。

### 方法 A: 寄付ボタンを作成する（推奨）
1. [PayPal 寄付ボタン作成ページ](https://www.paypal.com/donate/buttons) にアクセスします。
2. 「寄付ボタン」を選択し、指示に従って作成します。
3. 完了すると「ボタン ID（Hosted Button ID）」が発行されます。
4. アプリの `SettingsWindow.xaml` の以下の箇所に ID を記入します。
   - `https://www.paypal.com/donate/?hosted_button_id=ここにIDを入力`

### 方法 B: PayPal.Me を使用する（最も簡単）
1. [PayPal.Me](https://www.paypal.me/) で自分専用のリンク（例: `paypal.me/yourname`）を作成します。
2. その URL をアプリに直接設定します。
   - `SettingsWindow.xaml` の `CommandParameter` をこの URL に書き換えるだけで完了です。

---

## 3. アプリへの反映（コードの修正箇所）

手順書に従って取得した ID や URL は、プロジェクト内の以下のファイルを修正して反映させます。

- **ファイルパス**: `Views/SettingsWindow.xaml`
- **修正箇所**:
  - `500行目付近`: GitHub Sponsors リンク（3箇所の `ikaken` を自分の ID に変更）
  - `520行目付近`: PayPal リンク（`YOUR_PAYPAL_ID` を自分の ID に変更）

---

## 注意事項
- **本人確認**: いずれのサービスも、実際に日本円を受け取るためには銀行口座の登録と本人確認（KYC）が必要です。
- **税金**: 支援金は「贈与」や「事業所得」として扱われる可能性があるため、金額が大きくなる場合はお住まいの地域の税則を確認してください。
