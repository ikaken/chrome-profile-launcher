**# Windowsランチャーアプリ AdsJumbo広告実装仕様書**

## 1. 目的
本仕様書は、Windows常駐型ランチャーアプリにおいて**AdsJumbo SDK**を導入し、320×50バナー広告を実装するための設計・技術要件を定義する。  
**WebView2代替**として、ネイティブSDKによる軽量・高安定性・低リソース消費を実現する。

---

## 2. 対象アプリ特性
- 常駐型ランチャーアプリ（長時間起動前提）
- WPF（.NET 10.0 / .NET Framework 4.6.2以上）
- 画面下部固定表示（Settingsボタン左側）

---

## 3. 採用広告サービス
- **第一選択：AdsJumbo**（ネイティブSDK対応、100% fill rateを謳う）
- 補助：自社広告 or WebView2フォールバック（任意）

**採用理由**  
- WPF/WinFormsネイティブ対応（専用NuGetパッケージ）
- 軽量（Internetアクセス権限のみ）
- 320×50バナー対応
- Microsoft Advertising SDKの代替として実績あり

---

## 4. アカウント作成方法

1. **公式サイトにアクセス**  
   [https://adsjumbo.com/](https://adsjumbo.com/)

2. **登録**  
   - 「**Join Us Today!**」または「**Sign Up**」ボタンをクリック  
   - 登録ページ（`/account/register.php`）でメールアドレス・パスワード・必要情報を入力  
   - メール認証を実施

3. **ログイン後**  
   - Publisher Dashboardで**アプリを登録**  
   - アプリ情報を入力（アプリ名、プラットフォーム：WPF/Desktop、説明など）
   - **Request Activation**（審査依頼）を実行

4. **Ad Unit作成**  
   - 登録アプリを選択 → Ad Unit作成  
   - **320×50**（または対応サイズ）を選択  
   - **Application ID**（またはAd Unit ID）を取得

**注意**：審査通過後、広告が本稼働。審査期間は数日程度の場合が多い。

---

## 5. 広告サイズ・レイアウト（仕様書準拠）

- **採用サイズ**：320 × 50
- **コンテナ**：333 × 63（背景 `#1E1E1E`、枠 `1px #333`、角丸 `8px`）
- **配置**：ランチャー画面下部、Settingsボタン左側（余白12px以上）
- **視覚的分離**：区切り線 or 背景差 or カード化（必須）
- **広告ラベル**：「広告」（8〜10px、右上 or 左上）

---

## 6. 表示タイミング制御（仕様書準拠）

- 初期表示：起動後 **2秒遅延**
- 更新トリガー：
  - ランチャーボタン押下時（前回更新から**90秒以上**経過）
  - タイマー更新（7〜10分間隔）
- 表示条件：ウィンドウ前面・非最小化時のみ

---

## 7. 技術要件

### 7.1 使用技術
- **NuGetパッケージ**：`AdsJumbo.WinForm.WPF`（最新版）
- WPFコントロール：`BannerAds`

### 7.2 XAML配置例（MainWindow.xaml）
```xml
xmlns:ads="clr-namespace:AdsJumboWinForm;assembly=AdsJumboWinForm"

<Border ... >  <!-- コンテナ -->
    <ads:BannerAds x:Name="AdBanner"
                   Width="320" Height="50"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center" />
</Border>
```

### 7.3 C#実装例（AdHelper.cs推奨）
```csharp
public class AdHelper : IDisposable
{
    private readonly BannerAds _banner;
    private DateTime _lastUpdated = DateTime.MinValue;
    private const int MinIntervalSeconds = 90;
    private string _appId = "YOUR_APPLICATION_ID"; // Dashboardから取得

    public AdHelper(BannerAds banner)
    {
        _banner = banner;
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(2000); // 起動後2秒遅延
        await RefreshAdAsync();
    }

    public async Task RefreshAdAsync()
    {
        if ((DateTime.Now - _lastUpdated).TotalSeconds < MinIntervalSeconds) return;
        if (!IsWindowActive()) return;

        try
        {
            _lastUpdated = DateTime.Now;
            _banner.ShowAd(320, 50, _appId);  // 幅、高さ、App ID
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AdsJumbo Error: {ex.Message}");
            // 非表示 or フォールバック処理
        }
    }

    private bool IsWindowActive() { /* 前面・非最小化チェック */ }
}
```

---

## 8. 導入手順（ステップバイステップ）

1. **NuGetインストール**  
   `Install-Package AdsJumbo.WinForm.WPF`

2. **XAMLに名前空間・コントロール追加**

3. **Application ID設定**（Dashboardから取得）

4. **AdHelperクラス実装**（表示制御・更新ロジック）

5. **イベントハンドリング**（任意）  
   - `OnAdError`, `OnAdErrorNoAds` など

6. **テスト**  
   - Test Ad ID（Dashboard提供の場合）で動作確認  
   - 高DPI、最小化、複数更新の検証

7. **本番申請**  
   - アプリ登録 → Request Activation

---

## 9. パフォーマンス・禁止事項
- **推奨**：遅延ロード、再利用（コントロール再生成禁止）
- **禁止**：短時間更新（5〜60秒）、ユーザー操作直前切り替え、サイズ強制リサイズ
- Internetアクセス権限のみ（最小権限）

---

## 10. 収益最適化・拡張
- 操作連動＋低頻度更新でCTR重視
- A/Bテスト（位置・背景）
- フリーミアム対応（`IsAdVisible`プロパティでON/OFF）
- 自社広告ローテーション検討

---

## 11. 注意事項・サポート
- SDK最終更新は数年前だが、現在もWPF対応として利用可能。
- 問題発生時は `info@adsjumbo.com` へ連絡（スクリーンショット添付推奨）。
- eCPMはネットワーク状況による（100% fill rateが強み）。

---

**本仕様導入により**、WebView2比で**リソース消費削減・ネイティブ感向上・実装簡略化**が期待できます。

---

必要に応じて：
- 完全サンプルコード一式
- XAML詳細レイアウト
- エラーハンドリング拡張版

を提供します。実装前にPoC（テスト実装）をおすすめします！