# Privacy Policy / プライバシーポリシー

Last updated: 2026-11-12 / 最終更新日: 2026年11月12日

[English](#english) | [日本語](#日本語)

---

## English

Chrome Profile Launcher ("the App") is an open-source Windows desktop application developed by ikaken that lets you quickly launch Google Chrome with a specific profile.

### Summary

**The App does not collect, store, or transmit any personal information to the developer or to any third party.**

### Data the App accesses locally

The App reads and writes the following data **only on your computer**. None of it leaves your device.

| Data | Purpose |
|---|---|
| Google Chrome profile information (profile names, avatar icons) stored in Chrome's local user data directory | To display the list of profiles you can launch |
| App settings (profile order, window position, language, hotkey, startup preference) saved to `%APPDATA%\ChromeProfileLauncher\settings.json` | To remember your preferences |
| A local diagnostic log saved to `%APPDATA%\ChromeProfileLauncher\` | To help troubleshoot problems. The log is never sent anywhere automatically |

### Keyboard hotkey

The App uses a system-wide keyboard hook solely to detect a double tap of the hotkey you configure (for example the `Alt` key) so that the launcher window can be shown. The App does **not** record, store, or transmit any keystrokes.

### Network communication

- **Microsoft Store version**: The App makes **no network connections**. Updates are delivered by the Microsoft Store.
- **GitHub Releases version** (installer downloaded from GitHub): When automatic updates are enabled, the App contacts GitHub (`github.com`) to check for and download new versions. This is a standard HTTPS request that contains no personal information beyond what any web request includes (such as your IP address), and it is subject to [GitHub's Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement). You can turn this off in the App's settings.

### Third-party services

The App does not include advertising, analytics, or tracking SDKs of any kind.

### Children's privacy

The App does not knowingly collect any information from anyone, including children.

### Changes to this policy

Any changes will be published in this document in the project's GitHub repository.

### Contact

For questions about this policy, please open an issue at
https://github.com/ikaken/chrome-profile-launcher/issues

---

## 日本語

Chrome Profile Launcher（以下「本アプリ」）は、ikaken が開発するオープンソースの Windows デスクトップアプリケーションで、Google Chrome を特定のプロファイルで素早く起動するためのものです。

### 概要

**本アプリは、個人情報を収集・保存・送信することは一切なく、開発者や第三者へ情報を送ることもありません。**

### 本アプリがローカルで扱うデータ

本アプリは以下のデータを **お使いのコンピューター上でのみ** 読み書きします。これらがお使いの端末の外に出ることはありません。

| データ | 目的 |
|---|---|
| Google Chrome のローカルユーザーデータに保存されているプロファイル情報（プロファイル名・アバターアイコン） | 起動可能なプロファイル一覧を表示するため |
| アプリ設定（プロファイルの並び順、ウィンドウ位置、言語、ホットキー、スタートアップ設定）。`%APPDATA%\ChromeProfileLauncher\settings.json` に保存 | ユーザーの設定を記憶するため |
| 診断用ログ。`%APPDATA%\ChromeProfileLauncher\` に保存 | 問題の調査のため。自動的に外部へ送信されることはありません |

### キーボードホットキー

本アプリは、設定したホットキー（例: `Alt` キー）の 2 回押しを検知してランチャーウィンドウを表示するためだけに、システム全体のキーボードフックを使用します。キー入力の内容を記録・保存・送信することは **ありません**。

### ネットワーク通信

- **Microsoft Store 版**: 本アプリは **ネットワーク接続を一切行いません**。更新は Microsoft Store によって配信されます。
- **GitHub Releases 版**（GitHub からインストーラーをダウンロードした場合）: 自動更新が有効な場合、新しいバージョンの確認とダウンロードのために GitHub（`github.com`）へ接続します。これは通常の HTTPS 通信であり、一般的な Web アクセスに含まれる情報（IP アドレス等）以外の個人情報は含まれません。この通信は [GitHub のプライバシーに関する声明](https://docs.github.com/ja/site-policy/privacy-policies/github-privacy-statement) に従います。自動更新はアプリの設定画面でオフにできます。

### 第三者サービス

本アプリには、広告・アクセス解析・トラッキングのための SDK は一切含まれていません。

### 子どものプライバシー

本アプリは、子どもを含むいかなる人からも情報を収集しません。

### 本ポリシーの変更

変更がある場合は、プロジェクトの GitHub リポジトリ内の本ドキュメントで公開します。

### お問い合わせ

本ポリシーに関するご質問は、以下の Issue からお願いします。
https://github.com/ikaken/chrome-profile-launcher/issues
