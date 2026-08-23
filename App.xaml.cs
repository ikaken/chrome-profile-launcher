using System;
using System.IO;
using System.IO.Pipes;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ChromeProfileLauncher.Services;

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
    private KeyboardTriggerService? _triggerService;

    protected override void OnStartup(StartupEventArgs e)
    {
        var isFirstRun = string.Equals(
            Environment.GetEnvironmentVariable("VELOPACK_FIRSTRUN"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        // Velopack のセットアップ。アップデート後の再起動などをハンドルする。
        Velopack.VelopackApp.Build()
            .OnFirstRun(_ => isFirstRun = true)
            .Run();

        // 言語設定の読み込みと適用
        var settingsService = new Services.SettingsService(new Services.FileSystem());
        var settings = settingsService.LoadSettings();
        var language = isFirstRun ? GetInitialLanguage() : settings.Language;
        Helpers.LocalizationManager.SetLanguage(language);

        Helpers.Logger.Info($"Application starting... Language: {language}");
        
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

        if (isFirstRun)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                var firstRunViewModel = new ViewModels.FirstRunSetupViewModel(settingsService, new StartupService(), language);
                var firstRunWindow = new FirstRunSetupWindow(firstRunViewModel);
                firstRunWindow.ShowDialog();
                settings = settingsService.LoadSettings();
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("Failed to display first-run setup.", ex);
            }
        }

        // Named Pipe サーバーの開始
        StartPipeServer();

        // キーボードトリガーサービスの開始
        _triggerService = new KeyboardTriggerService(settings.HotkeyKey ?? "Alt");
        _triggerService.HotkeyDoubleTapped += (s, ev) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (MainWindow is MainWindow window)
                {
                    window.ShowAndActivate();
                }
            });
        };

        // MainWindow を作成
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        
        // 常にタスクバーに表示する（Issue #50 対応）
        mainWindow.ShowInTaskbar = true;
        Helpers.Logger.Info("MainWindow configured to show in taskbar.");

        mainWindow.Show();
        Helpers.Logger.Info($"MainWindow.Show() called. Window.ShowInTaskbar={mainWindow.ShowInTaskbar}");
    }

    private static string GetInitialLanguage()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase)
            ? "ja-JP"
            : "en-US";
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

    public void UpdateHotkeyKey(string hotkeyKey)
    {
        _triggerService?.UpdateHotkeyKey(hotkeyKey);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pipeCts?.Cancel();
        _triggerService?.Dispose();
        if (_mutex != null)
        {
            try { _mutex.ReleaseMutex(); } catch { }
            _mutex.Dispose();
        }
        base.OnExit(e);
    }
}
