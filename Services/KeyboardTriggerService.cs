using System;
using System.Diagnostics;
using System.Windows.Input;
using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher.Services;

public class KeyboardTriggerService : IDisposable
{
    private readonly KeyboardHookHelper _hook;
    private readonly Stopwatch _stopwatch = new();
    private int _ctrlPressCount;
    private const int DoubleClickTime = 300; // ms

    public event EventHandler? CtrlDoubleTapped;

    public KeyboardTriggerService()
    {
        _hook = new KeyboardHookHelper();
        _hook.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, Key key)
    {
        if (key == Key.LeftCtrl || key == Key.RightCtrl)
        {
            if (_ctrlPressCount == 0)
            {
                _stopwatch.Restart();
                _ctrlPressCount = 1;
            }
            else
            {
                if (_stopwatch.ElapsedMilliseconds < DoubleClickTime)
                {
                    CtrlDoubleTapped?.Invoke(this, EventArgs.Empty);
                    _ctrlPressCount = 0;
                    _stopwatch.Stop();
                }
                else
                {
                    _stopwatch.Restart();
                    _ctrlPressCount = 1;
                }
            }
        }
        else
        {
            _ctrlPressCount = 0;
            _stopwatch.Stop();
        }
    }

    public void Dispose()
    {
        _hook.KeyDown -= OnKeyDown;
        _hook.Dispose();
    }
}
