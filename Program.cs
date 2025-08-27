using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Z339xLib
{
    public static class Z339xLibSdk
    {
        // استدعاء الدوال من Z3272PStdLib.dll
        [DllImport("Z3272PStdLib.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetImageAndSaveFile(string portName, string fileName, int format);

        [DllImport("Z3272PStdLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetImageByBitmap(string portName);

        /// <summary>
        /// يحفظ الصورة مباشرة بالمسار المطلوب
        /// </summary>
        /// <param name="portName">مثلاً "AUTO" أو COM Port</param>
        /// <param name="fileName">المسار لحفظ الصورة</param>
        /// <param name="format">0=png, 1=bmp, 2=jpeg, 3=tiff</param>
        /// <returns>True إذا نجحت العملية</returns>
        public static bool CaptureAndSave(string portName, string fileName, int format = 2)
        {
            return GetImageAndSaveFile(portName, fileName, format);
        }

        /// <summary>
        /// يرجع الصورة كـ Bitmap Object 
        /// </summary>
        /// <param name="portName">مثلاً "AUTO" أو COM Port</param>
        /// <returns>Bitmap object أو null إذا صار خطأ</returns>
        public static Bitmap CaptureBitmap(string portName)
        {
            IntPtr bmpPtr = GetImageByBitmap(portName);
            if (bmpPtr == IntPtr.Zero)
                return null;

            Bitmap bmp = Image.FromHbitmap(bmpPtr);
            return bmp;
        }
    }
}

// =============================
// مثال للاستخدام
// =============================
class Program
{
    static void Main()
    {
        Console.WriteLine("🔌 Trying direct save with GetImageAndSaveFile...");
        string savePath = @"C:\Temp\capture.jpg";

        bool success = Z339xLib.Z339xLibSdk.CaptureAndSave("AUTO", savePath, 2); // JPEG
        if (success)
        {
            Console.WriteLine("✅ Saved directly: " + savePath);
        }
        else
        {
            Console.WriteLine("❌ Failed direct save. Trying GetImageByBitmap...");

            Bitmap bmp = Z339xLib.Z339xLibSdk.CaptureBitmap("AUTO");
            if (bmp != null)
            {
                string fallbackPath = @"C:\Temp\capture_fallback.jpg";
                bmp.Save(fallbackPath, ImageFormat.Jpeg);
                Console.WriteLine("✅ Captured via Bitmap: " + fallbackPath);
                bmp.Dispose();
            }
            else
            {
                Console.WriteLine("❌ CaptureBitmap returned null");
            }
        }
    }
}
