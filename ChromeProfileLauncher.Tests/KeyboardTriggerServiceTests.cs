using System;
using System.Threading;
using System.Windows.Input;
using ChromeProfileLauncher.Services;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class KeyboardTriggerServiceTests
    {
        private KeyboardTriggerService CreateService() => new KeyboardTriggerService(noHook: true);

        [Fact]
        public void AltDoubleTap_ShouldTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            // 2回目: KeyDown → ダブルタップ成立
            service.SimulateKeyDown(Key.LeftAlt);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void AltLongPress_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 長押し: KeyDown が連続で来る（キーリピート）
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート

            triggered.Should().BeFalse();
        }

        [Fact]
        public void AltLongPressAndRelease_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 長押し→離す: 1回のタップだけ
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyUp(Key.LeftAlt);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void AltDoubleTap_Timeout_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            // 300ms超の待機
            Thread.Sleep(350);

            // 2回目: KeyDown → タイムアウトのため不成立
            service.SimulateKeyDown(Key.LeftAlt);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void OtherKeyBetweenTaps_ShouldReset()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            // 別キーが割り込む
            service.SimulateKeyDown(Key.A);

            // 2回目: KeyDown → リセットされているため不成立
            service.SimulateKeyDown(Key.LeftAlt);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void RightAlt_ShouldAlsoWork()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.RightAlt);
            service.SimulateKeyUp(Key.RightAlt);
            service.SimulateKeyDown(Key.RightAlt);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void RepeatedDoubleTap_ShouldTriggerMultipleTimes()
        {
            using var service = CreateService();
            int count = 0;
            service.HotkeyDoubleTapped += (s, e) => count++;

            // 1回目のダブルタップ
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            // 2回目のダブルタップ
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            // 3回目のダブルタップ
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);

            count.Should().Be(3);
        }

        [Fact]
        public void CtrlDoubleTap_ShouldTrigger()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Ctrl");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void RightCtrl_ShouldAlsoWork()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Ctrl");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.RightCtrl);
            service.SimulateKeyUp(Key.RightCtrl);
            service.SimulateKeyDown(Key.RightCtrl);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void ShiftDoubleTap_ShouldTrigger()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Shift");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.LeftShift);
            service.SimulateKeyUp(Key.LeftShift);
            service.SimulateKeyDown(Key.LeftShift);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void RightShift_ShouldAlsoWork()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Shift");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.RightShift);
            service.SimulateKeyUp(Key.RightShift);
            service.SimulateKeyDown(Key.RightShift);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void UpdateHotkeyKey_ShouldChangeTriggerKey()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Alt");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.UpdateHotkeyKey("Ctrl");

            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void UpdateHotkeyKey_OldKey_ShouldNotTrigger()
        {
            using var service = new KeyboardTriggerService(noHook: true, hotkeyKey: "Alt");
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            service.UpdateHotkeyKey("Ctrl");

            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void DoubleTapAfterLongPress_ShouldTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.HotkeyDoubleTapped += (s, e) => triggered = true;

            // 長押し: KeyDown → キーリピート → KeyUp
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyDown(Key.LeftAlt); // キーリピート
            service.SimulateKeyUp(Key.LeftAlt);

            // 長押し後のダブルタップ
            service.SimulateKeyDown(Key.LeftAlt);
            service.SimulateKeyUp(Key.LeftAlt);
            service.SimulateKeyDown(Key.LeftAlt);

            triggered.Should().BeTrue();
        }
    }
}
