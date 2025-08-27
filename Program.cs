using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Z339xLib
{
    public class Z339xLibSdk
    {
        private const string DllName = "Z3272PStdLib.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern int OpenHID();

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern int CloseHID(int handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern int GetImageAndSaveFile(int handle, string filePath);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetImageByBitmap(int handle);

        [DllImport("gdi32.dll")]
        private static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        private int _handle;

        public bool Open()
        {
            _handle = OpenHID();
            Console.WriteLine($"🔌 OpenHID → {_handle}");
            return _handle > 0;
        }

        public void Close()
        {
            if (_handle > 0)
            {
                Console.WriteLine($"🔒 Closing handle {_handle}");
                CloseHID(_handle);
                _handle = 0;
            }
        }

        public bool CaptureToFile(string path)
        {
            Console.WriteLine("📸 Trying direct save with GetImageAndSaveFile...");
            int ret = GetImageAndSaveFile(_handle, path);
            Console.WriteLine($"GetImageAndSaveFile → {ret}");

            return ret == 1; // 1 يعني نجاح حسب convention DLL
        }

        public Bitmap CaptureBitmap()
        {
            Console.WriteLine("📸 Trying GetImageByBitmap...");

            IntPtr hBmp = GetImageByBitmap(_handle);
            if (hBmp == IntPtr.Zero)
            {
                Console.WriteLine("❌ GetImageByBitmap returned NULL");
                return null;
            }

            // تحقق من صلاحية الـ HBITMAP
            BITMAP bmpObj;
            int result = GetObject(hBmp, Marshal.SizeOf(typeof(BITMAP)), out bmpObj);

            if (result == 0 || bmpObj.bmWidth == 0 || bmpObj.bmHeight == 0)
            {
                Console.WriteLine("⚠️ Invalid HBITMAP returned from device.");
                DeleteObject(hBmp); // حرره عشان ما يضل leak
                return null;
            }

            Console.WriteLine($"HBITMAP → {bmpObj.bmWidth}x{bmpObj.bmHeight}, BitsPixel={bmpObj.bmBitsPixel}");

            try
            {
                Bitmap bmp = Image.FromHbitmap(hBmp);

                // مهم: اعمل نسخة جديدة عشان ما تعتمد على الهاندل unmanaged
                Bitmap clone = new Bitmap(bmp);

                bmp.Dispose();
                DeleteObject(hBmp); // حرر الهاندل unmanaged

                return clone;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create Bitmap from HBITMAP: {ex.Message}");
                DeleteObject(hBmp);
                return null;
            }
        }
    }
}
