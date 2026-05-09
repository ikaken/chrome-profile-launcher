using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChromeProfileLauncher.Helpers
{
    public class IconPathToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            string? path = value as string;
            if (string.IsNullOrEmpty(path)) return null;

            try
            {
                // EXEファイルのアイコン抽出
                if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return ExtractIconFromExe(path);
                }

                // Pack URI または 通常のファイルパス
                var uri = new Uri(path, path.StartsWith("pack://") ? UriKind.Absolute : UriKind.RelativeOrAbsolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }

        private ImageSource? ExtractIconFromExe(string path)
        {
            var shfi = new Win32Api.SHFILEINFO();
            var res = Win32Api.SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), Win32Api.SHGFI_ICON | Win32Api.SHGFI_LARGEICON);

            if (res != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
            {
                try
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        shfi.hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    
                    return bitmapSource;
                }
                finally
                {
                    Win32Api.DestroyIcon(shfi.hIcon);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
