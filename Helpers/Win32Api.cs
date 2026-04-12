using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ChromeProfileLauncher.Helpers
{
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        // --- Shell Property Store (AUMID取得用) ---

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out uint cProps);
            [PreserveSig]
            int GetAt(uint iProp, out PropertyKey pkey);
            [PreserveSig]
            int GetValue(ref PropertyKey key, out PropVariant pv);
            [PreserveSig]
            int SetValue(ref PropertyKey key, ref PropertyKey pv); // Simplified for read-only use
            [PreserveSig]
            int Commit();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;

            public static readonly PropertyKey PKEY_AppUserModel_ID = new PropertyKey
            {
                fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
                pid = 5
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PropVariant : IDisposable
        {
            public ushort vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;
            public IntPtr ptr;

            public void Dispose()
            {
                PropVariantClear(ref this);
            }

            [DllImport("ole32.dll")]
            private static extern int PropVariantClear(ref PropVariant pvar);

            public string GetValue()
            {
                if (vt == 31) // VT_LPWSTR
                {
                    return Marshal.PtrToStringUni(ptr) ?? string.Empty;
                }
                return string.Empty;
            }
        }

        public static readonly Guid IID_IPropertyStore = new Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99");
    }
}
