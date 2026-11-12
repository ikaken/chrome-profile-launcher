using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ChromeProfileLauncher.Helpers;

public static class PackagedAppHelper
{
    private const int AppModelErrorNoPackage = 15700;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

    private static readonly Lazy<bool> _isPackaged = new(() =>
    {
        try
        {
            int len = 0;
            return GetCurrentPackageFullName(ref len, null) != AppModelErrorNoPackage;
        }
        catch
        {
            return false;
        }
    });

    public static bool IsPackaged => _isPackaged.Value;
}
