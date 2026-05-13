using System;
using System.Windows;
using ChromeProfileLauncher.Services;
using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher;

public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService(new FileSystem());
        SourceInitialized += MainWindow_SourceInitialized;
        InitializeAdWebView();
    }

    private async void InitializeAdWebView()
    {
        await AdWebView.EnsureCoreWebView2Async();
        // テスト用のダミー広告ページ（Bing検索ページを代用）
        AdWebView.Source = new Uri("https://www.bing.com");
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var pos = _settingsService.LoadWindowPosition();
        if (WindowPositionHelper.IsPositionValid(pos.left, pos.top, pos.width, pos.height))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = pos.left;
            Top = pos.top;
            Width = pos.width;
            Height = pos.height;
            if (pos.isMaximized) WindowState = WindowState.Maximized;
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, false);
        }
        else if (WindowState == WindowState.Maximized)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, true);
        }
    }
}
