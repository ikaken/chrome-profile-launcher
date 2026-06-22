using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var settings = _settingsService.LoadSettings();
        Logger.Info($"MainWindow_Loaded: EnableTaskTray={settings.EnableTaskTray}");
        if (settings.EnableTaskTray)
        {
            Logger.Info("MainWindow_Loaded: Enabling TaskTray.");
            InitializeTaskbarIcon();
        }
        else
        {
            Logger.Info("MainWindow_Loaded: Skipping TaskTray initialization.");
        }
    }

    public void InitializeTaskbarIcon()
    {
        Logger.Info("InitializeTaskbarIcon: Starting icon generation.");

        // 既に存在していれば何もしない
        if (this.Content is Grid grid && grid.Children.OfType<H.NotifyIcon.TaskbarIcon>().Any())
        {
            Logger.Info("InitializeTaskbarIcon: Icon already exists. Skipping.");
            return;
        }

        var icon = new H.NotifyIcon.TaskbarIcon();
        Logger.Info("InitializeTaskbarIcon: New TaskbarIcon instance created.");
        
        icon.IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ChromeProfileLauncher;component/Assets/app.ico"));
        icon.ToolTipText = "Chrome Profile Launcher";
        
        var contextMenu = new System.Windows.Controls.ContextMenu();
        var menuOpen = new System.Windows.Controls.MenuItem { Header = "ランチャを開く", FontWeight = FontWeights.Bold };
        menuOpen.Click += (s, ev) => Dispatcher.Invoke(ShowAndActivate);
        contextMenu.Items.Add(menuOpen);
        
        var menuSettings = new System.Windows.Controls.MenuItem { Header = "設定" };
        menuSettings.Click += (s, ev) => (DataContext as ViewModels.MainViewModel)?.SettingsCommand.Execute(null);
        contextMenu.Items.Add(menuSettings);
        
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        
        var menuExit = new System.Windows.Controls.MenuItem { Header = "終了" };
        menuExit.Click += (s, ev) => (DataContext as ViewModels.MainViewModel)?.ExitApplicationCommand.Execute(null);
        contextMenu.Items.Add(menuExit);
        
        icon.ContextMenu = contextMenu;
        icon.TrayLeftMouseDown += (s, ev) => Dispatcher.Invoke(ShowAndActivate);
        icon.TrayLeftMouseDoubleClick += (s, ev) => Dispatcher.Invoke(ShowAndActivate);
        
        if (Content is Grid g)
        {
            g.Children.Add(icon);
            Logger.Info("InitializeTaskbarIcon: Added icon to Grid.");
        }
    }

    public void RemoveTaskbarIcon()
    {
        if (this.Content is Grid grid)
        {
            var icons = grid.Children.OfType<H.NotifyIcon.TaskbarIcon>().ToList();
            foreach (var icon in icons)
            {
                icon.Dispose();
                grid.Children.Remove(icon);
            }
            Logger.Info("RemoveTaskbarIcon: Removed and disposed taskbar icon.");
        }
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
        Logger.Info($"Window_Closing: EnableTaskTray={settings.EnableTaskTray}, WindowState={WindowState}");

        // タスクトレイ常駐が有効な場合、終了をキャンセルして非表示にする
        if (settings.EnableTaskTray)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.Visibility = Visibility.Collapsed;
            }
            Logger.Info("Window_Closing: Cancelled close and collapsed window (staying in tray).");
            return;
        }

        // 常駐が無効な場合、位置を保存してアプリケーションを終了する
        Logger.Info("Window_Closing: EnableTaskTray is False. Terminating application.");

        if (WindowState == WindowState.Normal)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, false);
        }
        else if (WindowState == WindowState.Maximized)
        {
            _settingsService.SaveWindowPosition(Left, Top, Width, Height, true);
        }

        Application.Current.Shutdown();
    }

    /// <summary>
    /// ウィンドウを表示し、最前面に持ってきます。
    /// 表示ラグを最小限に抑えるため、プロパティ変更のガードと Win32 API を活用します。
    /// 裏に隠れている状態からでも確実に出すため、フォアグラウンドスレッドのアタッチを行います。
    /// </summary>
    public void ShowAndActivate()
    {
        Logger.Info("ShowAndActivate: Process started.");

        // 1. 表示設定 (ガード条件付き)
        if (this.Visibility != Visibility.Visible)
        {
            this.Visibility = Visibility.Visible;
        }

        // 常にタスクバーに表示する（Issue #50 対応）
        if (this.ShowInTaskbar != true)
        {
            this.ShowInTaskbar = true;
        }

        // ViewModel の状態も同期
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.Visibility = Visibility.Visible;
            vm.ShowInTaskbar = true;
        }

        this.Show();

        // 2. ウィンドウ状態の復元
        if (this.WindowState == WindowState.Minimized)
        {
            this.WindowState = WindowState.Normal;
        }

        // 3. 最前面化 (Win32 API)
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            // 強制的に前面に出すためのアタッチ処理
            IntPtr foregroundHwnd = Win32Api.GetForegroundWindow();
            uint foregroundThreadId = Win32Api.GetWindowThreadProcessId(foregroundHwnd, out _);
            uint currentThreadId = Win32Api.GetCurrentThreadId();

            if (foregroundThreadId != currentThreadId)
            {
                Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, true);
                Win32Api.SetForegroundWindow(hwnd);
                Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }
            else
            {
                Win32Api.SetForegroundWindow(hwnd);
            }

            Win32Api.ShowWindow(hwnd, Win32Api.SW_RESTORE);
        }

        // 4. フォーカス処理
        this.Activate();
        this.Focus();

        Logger.Info("ShowAndActivate: Finished.");
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        Logger.Info($"Window_StateChanged: New State={WindowState}, Current Visibility={Visibility}");

        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.WindowState = WindowState;
        }

        // 最小化時のタスクトレイ格納ロジックを削除（Issue #50 修正案）
        // 常駐設定時でも、最小化ボタンではタスクバーに残るようにする
    }
}
