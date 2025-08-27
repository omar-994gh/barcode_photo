using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Z339xLib
{
    public static class Z339xLibSdk
    {
        // استدعاء الدوال من Z3272PStdLib.dll
        [DllImport("Z3272PStdLib.dll", EntryPoint = "GetImageAndSaveFile", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Native_GetImageAndSaveFile(string portName, string fileName, int format);

        [DllImport("Z3272PStdLib.dll", EntryPoint = "GetImageByBitmap", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr Native_GetImageByBitmap(string portName);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// يحفظ الصورة مباشرة بالمسار المطلوب
        /// </summary>
        /// <param name="portName">مثلاً "AUTO" أو COM Port</param>
        /// <param name="fileName">المسار لحفظ الصورة</param>
        /// <param name="format">0=png, 1=bmp, 2=jpeg, 3=tiff</param>
        /// <returns>True إذا نجحت العملية</returns>
        public static bool CaptureAndSave(string portName, string fileName, int format = 2)
        {
            return Native_GetImageAndSaveFile(portName, fileName, format);
        }

        /// <summary>
        /// يرجع الصورة كـ Bitmap Object 
        /// </summary>
        /// <param name="portName">مثلاً "AUTO" أو COM Port</param>
        /// <returns>Bitmap object أو null إذا صار خطأ</returns>
        public static Bitmap CaptureBitmap(string portName)
        {
            IntPtr bmpPtr = Native_GetImageByBitmap(portName);
            if (bmpPtr == IntPtr.Zero)
                return null;

            Bitmap bmp = Image.FromHbitmap(bmpPtr);
            // Free unmanaged HBITMAP returned by the native DLL
            DeleteObject(bmpPtr);
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
        string saveDir = @"C:\\Temp";
        string savePath = @"C:\\Temp\\capture.jpg";

        try
        {
            if (!System.IO.Directory.Exists(saveDir))
            {
                System.IO.Directory.CreateDirectory(saveDir);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Cannot ensure save directory '{saveDir}': {ex.Message}");
        }

        bool success = Z339xLib.Z339xLibSdk.CaptureAndSave("AUTO", savePath, 2); // JPEG
        if (success)
        {
            Console.WriteLine("✅ Saved directly: " + savePath);
        }
        else
        {
            Console.WriteLine("❌ Direct save failed.");
        }
    }
}
