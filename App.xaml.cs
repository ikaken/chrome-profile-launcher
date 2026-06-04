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
        // 言語設定の読み込みと適用
        var settingsService = new Services.SettingsService(new Services.FileSystem());
        var settings = settingsService.LoadSettings();
        Helpers.LocalizationManager.SetLanguage(settings.Language);

        // Velopack のセットアップ。アップデート後の再起動などをハンドルする。
        Velopack.VelopackApp.Build().Run();

        Helpers.Logger.Info($"Application starting... Language: {settings.Language}");
        
        // Mutex の取得を試める
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
        }

        Helpers.Logger.Info("Starting as primary instance.");
        base.OnStartup(e);

        // Named Pipe サーバーの開始
        StartPipeServer();

        // MainWindow を作成
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
        Helpers.Logger.Info("Activating MainWindow requested via IPC.");
        if (MainWindow is MainWindow window)
        {
            window.ShowAndActivate();
        }
        else
        {
            Helpers.Logger.Error("ActivateWindow: MainWindow is null or not of type MainWindow.");
        }
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
