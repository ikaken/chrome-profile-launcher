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
        public void CtrlDoubleTap_ShouldTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            // 2回目: KeyDown → ダブルタップ成立
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void CtrlLongPress_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 長押し: KeyDown が連続で来る（キーリピート）
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート

            triggered.Should().BeFalse();
        }

        [Fact]
        public void CtrlLongPressAndRelease_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 長押し→離す: 1回のタップだけ
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyUp(Key.LeftCtrl);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void CtrlDoubleTap_Timeout_ShouldNotTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            // 300ms超の待機
            Thread.Sleep(350);

            // 2回目: KeyDown → タイムアウトのため不成立
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void OtherKeyBetweenTaps_ShouldReset()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 1回目: KeyDown → KeyUp
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            // 別キーが割り込む
            service.SimulateKeyDown(Key.A);

            // 2回目: KeyDown → リセットされているため不成立
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeFalse();
        }

        [Fact]
        public void RightCtrl_ShouldAlsoWork()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            service.SimulateKeyDown(Key.RightCtrl);
            service.SimulateKeyUp(Key.RightCtrl);
            service.SimulateKeyDown(Key.RightCtrl);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void RepeatedDoubleTap_ShouldTriggerMultipleTimes()
        {
            using var service = CreateService();
            int count = 0;
            service.CtrlDoubleTapped += (s, e) => count++;

            // 1回目のダブルタップ
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            // 2回目のダブルタップ
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            // 3回目のダブルタップ
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);

            count.Should().Be(3);
        }

        [Fact]
        public void DoubleTapAfterLongPress_ShouldTrigger()
        {
            using var service = CreateService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 長押し: KeyDown → キーリピート → KeyUp
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyDown(Key.LeftCtrl); // キーリピート
            service.SimulateKeyUp(Key.LeftCtrl);

            // 長押し後のダブルタップ
            service.SimulateKeyDown(Key.LeftCtrl);
            service.SimulateKeyUp(Key.LeftCtrl);
            service.SimulateKeyDown(Key.LeftCtrl);

            triggered.Should().BeTrue();
        }
    }
}
