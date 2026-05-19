using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChromeProfileLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = "Global\\ChromeProfileLauncher-SingleInstance-Mutex";
    private const string PipeName = "ChromeProfileLauncher-SingleInstance-Pipe";
    private CancellationTokenSource? _pipeCts;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Velopack のセットアップ。アップデート後の再起動などをハンドルする。
        Velopack.VelopackApp.Build().Run();

        Helpers.Logger.Info("Application starting...");
        
        // Mutex の取得を試みる
        _mutex = new Mutex(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            Helpers.Logger.Info("Another instance might be running. Attempting IPC to activate it.");
            
            // 既存インスタンスへ通知を試みる
            if (SendMessageToExistingInstance("ACTIVATE"))
            {
                Helpers.Logger.Info("Successfully notified existing instance. Shutting down.");
                _mutex.Dispose();
                _mutex = null;
                Shutdown();
                return;
            }

            Helpers.Logger.Info("IPC failed (zombie process?). Proceeding as primary instance.");
            // IPCに失敗した場合（ゾンビプロセスの可能性）、自分をプライマリとして続行
            // この場合 Mutex は前のプロセスが持っているが、強制的に奪うことはできないため
            // 既存の Mutex を閉じて、新しく取得し直す（あるいはそのまま進む）
            // 実際には Mutex が残っていると new Mutex(true, ...) で createdNew が false になり続ける
            // ここでは簡易的に「IPCが通らなければ起動を許可する」方針とする
        }

        Helpers.Logger.Info("Starting as primary instance.");
        base.OnStartup(e);

        // Named Pipe サーバーの開始
        StartPipeServer();

        // MainWindow を作成
        var settings = new Services.SettingsService(new Services.FileSystem()).LoadSettings();
        var mainWindow = new MainWindow();
        
        // 常駐オンなら隠しオーナーを作成してタスクバーを隠す
        if (settings.EnableTaskTray)
        {
            var ownerWindow = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                Opacity = 0
            };
            ownerWindow.Show();
            mainWindow.Owner = ownerWindow;
            mainWindow.ShowInTaskbar = false;
            Helpers.Logger.Info("Configured for task tray (ShowInTaskbar=false).");
        }
        else
        {
            mainWindow.ShowInTaskbar = true;
            Helpers.Logger.Info("Configured for task bar (ShowInTaskbar=true).");
        }

        mainWindow.Show();
        Helpers.Logger.Info($"MainWindow.Show() called. Window.ShowInTaskbar={mainWindow.ShowInTaskbar}");
    }

    private bool SendMessageToExistingInstance(string message)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            // 200ms タイムアウト（設計書通り）
            client.Connect(200);

            using var writer = new StreamWriter(client, Encoding.UTF8);
            writer.Write(message);
            writer.Flush();
            return true;
        }
        catch (Exception ex)
        {
            Helpers.Logger.Info($"Failed to connect to existing instance via IPC: {ex.Message}");
            return false;
        }
    }

    private void StartPipeServer()
    {
        _pipeCts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            try
            {
                while (!_pipeCts.Token.IsCancellationRequested)
                {
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await server.WaitForConnectionAsync(_pipeCts.Token);

                    using var reader = new StreamReader(server, Encoding.UTF8);
                    string? message = await reader.ReadToEndAsync(_pipeCts.Token);

                    if (message == "ACTIVATE")
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            ActivateWindow();
                        });
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Helpers.Logger.Error($"Named Pipe Server error: {ex.Message}", ex);
            }
        }, _pipeCts.Token);
    }

    private void ActivateWindow()
    {
        var window = MainWindow;
        if (window == null) return;

        Helpers.Logger.Info("Activating MainWindow requested via IPC.");

        // 1. まず表示状態にする
        window.ShowInTaskbar = false;
        window.Visibility = Visibility.Visible;
        window.Show();

        // 2. 復元処理 (Win32 API 併用)
        window.WindowState = WindowState.Normal;
        
        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        if (hwnd != IntPtr.Zero)
        {
            Helpers.Win32Api.ShowWindow(hwnd, Helpers.Win32Api.SW_RESTORE);
            Helpers.Win32Api.SetForegroundWindow(hwnd);
        }

        // 3. ViewModel のプロパティも同期させる
        if (window.DataContext is ViewModels.MainViewModel vm)
        {
            vm.Visibility = Visibility.Visible;
            vm.WindowState = window.WindowState;
            vm.ShowInTaskbar = false;
        }

        // 4. アクティブ化
        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pipeCts?.Cancel();
        if (_mutex != null)
        {
            try { _mutex.ReleaseMutex(); } catch { }
            _mutex.Dispose();
        }
        base.OnExit(e);
    }
}
