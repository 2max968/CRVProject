using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CRVProject.Ortsschild.WinForms
{
    public static class ImageConverter
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public static unsafe Bitmap Mat2Bitmap(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out byte[] buffer);
            using MemoryStream ms = new MemoryStream(buffer);
            return new Bitmap(ms);
        }
    }
}
