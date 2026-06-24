using System;
using System.Diagnostics;
using System.Windows.Input;
using ChromeProfileLauncher.Helpers;

namespace ChromeProfileLauncher.Services;

public class KeyboardTriggerService : IDisposable
{
    private readonly KeyboardHookHelper _hook;
    private readonly Stopwatch _stopwatch = new();
    private bool _firstTapCompleted;
    private bool _keyHeld;
    private bool _ignoreNextKeyUp;
    private string _hotkeyKey;
    private const int DoubleClickTime = 300; // ms

    public event EventHandler? HotkeyDoubleTapped;

    public KeyboardTriggerService(string hotkeyKey = "Alt")
    {
        _hotkeyKey = hotkeyKey;
        _hook = new KeyboardHookHelper();
        _hook.KeyDown += OnKeyDown;
        _hook.KeyUp += OnKeyUp;
    }

    public void UpdateHotkeyKey(string hotkeyKey)
    {
        _hotkeyKey = hotkeyKey;
        Reset();
    }

    // テスト用コンストラクタ（フックなし）
    internal KeyboardTriggerService(bool noHook, string hotkeyKey = "Alt")
    {
        _hotkeyKey = hotkeyKey;
        _hook = null!;
    }

    internal void SimulateKeyDown(Key key) => OnKeyDown(null, key);
    internal void SimulateKeyUp(Key key) => OnKeyUp(null, key);

    private bool IsTargetKey(Key key) => _hotkeyKey switch
    {
        "None"  => false,
        "Ctrl"  => key == Key.LeftCtrl  || key == Key.RightCtrl,
        "Shift" => key == Key.LeftShift || key == Key.RightShift,
        _       => key == Key.LeftAlt   || key == Key.RightAlt,
    };

    private void OnKeyDown(object? sender, Key key)
    {
        if (IsTargetKey(key))
        {
            if (_keyHeld)
                return; // キーリピートを無視

            _keyHeld = true;

            if (_firstTapCompleted)
            {
                if (_stopwatch.ElapsedMilliseconds < DoubleClickTime)
                {
                    // 2回目のKeyDown → ダブルタップ成立
                    HotkeyDoubleTapped?.Invoke(this, EventArgs.Empty);
                    _ignoreNextKeyUp = true;
                }
                // 成立・不成立に関わらず、タップサイクルをリセット
                _firstTapCompleted = false;
                _stopwatch.Stop();
            }
        }
        else
        {
            Reset();
        }
    }

    private void OnKeyUp(object? sender, Key key)
    {
        if (IsTargetKey(key))
        {
            _keyHeld = false;

            if (_ignoreNextKeyUp)
            {
                _ignoreNextKeyUp = false;
                return;
            }

            if (!_firstTapCompleted)
            {
                // タップ完了（KeyDown→KeyUp）→ 次のダブルタップ判定の起点
                _firstTapCompleted = true;
                _stopwatch.Restart();
            }
        }
    }

    private void Reset()
    {
        _firstTapCompleted = false;
        _keyHeld = false;
        _ignoreNextKeyUp = false;
        _stopwatch.Stop();
    }

    public void Dispose()
    {
        if (_hook != null)
        {
            _hook.KeyDown -= OnKeyDown;
            _hook.KeyUp -= OnKeyUp;
            _hook.Dispose();
        }
    }
}
