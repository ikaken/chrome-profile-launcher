using System.Windows;

namespace ChromeProfileLauncher.Helpers
{
    public static class WindowPositionHelper
    {
        public static bool IsPositionValid(double left, double top, double width, double height)
        {
            Rect windowRect = new Rect(left, top, width, height);
            Rect screenRect = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight
            );

            // ウィンドウの一部がスクリーン内にあれば有効とみなす
            return windowRect.IntersectsWith(screenRect);
        }

        public static bool IsSizeValid(double width, double height)
        {
            return width >= 200 && height >= 150;
        }
    }
}
