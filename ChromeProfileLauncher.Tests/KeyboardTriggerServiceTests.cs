using System;
using System.Windows.Input;
using ChromeProfileLauncher.Services;
using Xunit;
using FluentAssertions;

namespace ChromeProfileLauncher.Tests
{
    public class KeyboardTriggerServiceTests
    {
        [Fact]
        public void OnKeyDown_ShouldDetectCtrlDoubleTap()
        {
            // Arrange
            using var service = new KeyboardTriggerService();
            bool triggered = false;
            service.CtrlDoubleTapped += (s, e) => triggered = true;

            // 擬似的にKeyDownイベントを発生させるのはサービス内部のフックがグローバルなため難しい。
            // しかし、ロジック部分はテスト可能にする必要がある。
            // サービスをテストしやすくするためにロジックをリファクタリングするか、
            // 現在の構成で可能な範囲をテストする。
        }
    }
}
