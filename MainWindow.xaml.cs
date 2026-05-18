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
        var settings = _settingsService.LoadSettings();

        // トレイ常駐が有効な場合、終了をキャンセルして非表示にする
        if (settings.CloseToTray)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.Visibility = Visibility.Collapsed;
            }
            return;
        }

        if (WindowState == WindowState.Normal)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, false);
        }
        else if (WindowState == WindowState.Maximized)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, true);
        }
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.WindowState = WindowState;
        }

        if (WindowState == WindowState.Minimized)
        {
            var settings = _settingsService.LoadSettings();
            if (settings.MinimizeToTray)
            {
                this.Visibility = Visibility.Collapsed;
                if (DataContext is ViewModels.MainViewModel vm2)
                {
                    vm2.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
