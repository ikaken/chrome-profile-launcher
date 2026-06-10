# ドネーション（開発支援）設定手順書

このドキュメントでは、Chrome Profile Launcher のドネーション機能で使用する **GitHub Sponsors** と **Ko-fi** の設定方法を解説します。

---

## 1. GitHub Sponsors の設定方法

GitHub Sponsors は、継続的な支援を得やすい月額サポートを推奨します。日本から利用できる場合は、まずこちらをメインの支援手段としてください。

### ステップ 1: プログラムへの参加
1. [GitHub Sponsors](https://github.com/sponsors) にアクセスします。
2. 「Get sponsored」をクリックし、自身のアカウントを選択します。
3. 支払先情報（Stripe アカウントの作成が必要）やプロフィールの設定を完了させます。
   - ※ 審査に数日かかる場合があります。

### ステップ 2: 支援ティアの設定
1. 管理画面の「Sponsorship tiers」から「Add a tier」をクリックします。
2. 「Monthly payment（月額）」を中心に設定することを推奨します。
3. 必要に応じて「One-time payment（一回払い）」のティアも追加できます。
4. アプリの案内では、月額支援をメインにしつつ、一回払いも選べることを明示します。

### ステップ 3: リンクの取得とアプリへの反映
- あなたの Sponsors ページの URL は `https://github.com/sponsors/ikaken` になります。
- まずはこのページへのリンクを `SettingsWindow.xaml` に設定します。
- 追加の一回払いリンクを表示する場合は、GitHub Sponsors の一回払いティアリンクを個別に設定できます。

---

## 2. Ko-fi の設定方法

Ko-fi は気軽な一回払い支援用のサブ手段としてお勧めします。GitHub Sponsors をメインとし、Ko-fi は「まずは一回だけ支援したい人」向けの補助として扱います。

### ステップ 1: Ko-fi に登録
1. [Ko-fi](https://ko-fi.com/) にアクセスします。
2. アカウントを作成し、プロフィールを設定します。
3. 支援ページの URL が発行されます（例: `https://ko-fi.com/ikaken`）。

### ステップ 2: 支援リンクの反映
- Ko-fi の支援ページ URL を、アプリの `Views/SettingsWindow.xaml` に設定します。
- 例: `https://ko-fi.com/ikaken`

### ステップ 3: 受け取り方法の設定
- Ko-fi は Stripe との連携で受け取りが可能です。
- 既に Stripe アカウントを持っている場合は、Ko-fi の設定画面から Stripe を接続してください。

---

## 3. PayPal の扱いについて

現在のところ、日本の PayPal アカウントでは寄付ページ作成が利用できない場合があります。そのため、PayPal は本手順書のメイン対象から外しています。

- PayPal を使わず、`GitHub Sponsors` をメイン、`Ko-fi` をサブの支援構成としてください。
- 代替として、`Stripe Payment Links` を検討することもできます。

---

## 4. アプリへの反映（コードの修正箇所）

手順書に従って取得した ID や URL は、プロジェクト内の以下のファイルを修正して反映させます。

- **ファイルパス**: `Views/SettingsWindow.xaml`
- **修正箇所**:
  - `500行目付近`: GitHub Sponsors リンク（`ikaken` を自分の ID に変更）
  - `520行目付近`: Ko-fi / Stripe リンク（`YOUR_PAYPAL_ID` の代わりに Ko-fi または Stripe の URL を設定）

---

## 注意事項
- **本人確認**: いずれのサービスも、実際に日本円を受け取るためには銀行口座の登録と本人確認（KYC）が必要です。
- **税金**: 支援金は「贈与」や「事業所得」として扱われる可能性があるため、金額が大きくなる場合はお住まいの地域の税則を確認してください。
