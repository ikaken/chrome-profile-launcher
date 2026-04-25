using System;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace ChromeProfileLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = "ChromeProfileLauncher-SingleInstance-Mutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        Helpers.Logger.Info("Application starting...");
        _mutex = new Mutex(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            Helpers.Logger.Info("Another instance is already running. Activating existing window and shutting down.");
            // 既に起動している場合、既存のウィンドウを探して最前面に表示する
            ActivateExistingWindow();
            
            // 現在のインスタンスを終了
            _mutex.Dispose();
            _mutex = null;
            Shutdown();
            return;
        }

        Helpers.Logger.Info("Mutex acquired. This is the primary instance.");
        base.OnStartup(e);

        // MainWindow を手動で作成して表示
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Helpers.Logger.Info("MainWindow shown.");
    }

    private void ActivateExistingWindow()
    {
        IntPtr hWnd = NativeMethods.FindWindow(null, "Chrome Profile Launcher");
        if (hWnd != IntPtr.Zero)
        {
            if (NativeMethods.IsIconic(hWnd))
            {
                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
            }
            NativeMethods.SetForegroundWindow(hWnd);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
        base.OnExit(e);
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        public const int SW_RESTORE = 9;
    }
}
