using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject
{
    public static class Win32
    {
        public const int GWL_STYLE = -16;
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_SYSMENU = 0x00080000;

        [DllImport("User32")]
        public static extern int GetWindowLongW(IntPtr hWnd, int nIndex);
        [DllImport("User32")]
        public static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int dwLong);
    }
}
