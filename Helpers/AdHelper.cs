using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChromeProfileLauncher.Helpers
{
    public class AdHelper
    {
        private DispatcherTimer _timer;
        private DateTime _lastUpdate;
        private const int UpdateIntervalMinutes = 7;
        private const int MinUpdateSeconds = 90;

        public event Action<bool> AdVisibilityChanged;

        public AdHelper()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(UpdateIntervalMinutes);
            _timer.Tick += (s, e) => RequestUpdate();
            _timer.Start();
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(2000); // 初期表示の遅延
            AdVisibilityChanged?.Invoke(true);
            _lastUpdate = DateTime.Now;
        }

        public void RequestUpdate()
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds < MinUpdateSeconds)
                return;

            // ここに更新ロジックを実装（現在はシミュレーション）
            _lastUpdate = DateTime.Now;
        }
    }
}
