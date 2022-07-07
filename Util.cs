using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Eva.Util
{
    public class Screen
    {
        public static Stream Capture(ImageFormat format)
        {
            MemoryStream stream = new();
            Image image = CaptureWindow(User32.GetDesktopWindow());
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public static string CaptureAsBase64(ImageFormat format)
        {
            MemoryStream stream = new();
            Image image = CaptureWindow(User32.GetDesktopWindow());
            image.Save(stream, format);
            stream.Position = 0;
            byte[] imageBytes = stream.ToArray();
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public static Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);

            return img;
        }

        public static void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        }
    }

    public static class MessageBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        public static MessageBoxResult Show(string text)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, "\0", (uint)MessageBoxButtons.Ok);
        }

        public static MessageBoxResult Show(string text, string caption)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)MessageBoxButtons.Ok);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.Ok)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)buttons);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon));
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button));
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button, MessageBoxModal modal)
        {
            return (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button) | ((uint)modal));
        }
    }

    public enum MessageBoxButtons
    {
        AbortRetryIgnore = 0x00000002,
        CancelTryIgnore = 0x00000006,
        Help = 0x00004000,
        Ok = 0x00000000,
        OkCancel = 0x00000001,
        RetryCancel = 0x00000005,
        YesNo = 0x00000004,
        YesNoCancel = 0x00000003
    }

    public enum MessageBoxResult
    {
        Abort = 3,
        Cancel = 2,
        Continue = 11,
        Ignore = 5,
        No = 7,
        Ok = 1,
        Retry = 10,
        Yes = 6
    }

    public enum MessageBoxDefaultButton : uint
    {
        Button1 = 0x00000000,
        Button2 = 0x00000100,
        Button3 = 0x00000200,
        Button4 = 0x00000300,
    }

    public enum MessageBoxModal : uint
    {
        Application = 0x00000000,
        System = 0x00001000,
        Task = 0x00002000
    }

    public enum MessageBoxIcon : uint
    {
        Warning = 0x00000030,
        Information = 0x00000040,
        Question = 0x00000020,
        Error = 0x00000010
    }

    public class Utilities
    {
        public class Lookup
        {
            public double Value;
            public string Symbol;
        }
        public static string NumberFormatter(int num, int digits = 0)
        {
            List<Lookup> lookup = new List<Lookup> {
                new Lookup { Value = 1, Symbol = "" },
                new Lookup { Value = 1e3, Symbol = "k" },
                new Lookup { Value = 1e6, Symbol = "M" },
                new Lookup { Value = 1e9, Symbol = "G" },
                new Lookup { Value = 1e12, Symbol = "T" },
                new Lookup { Value = 1e15, Symbol = "P" },
                new Lookup { Value = 1e18, Symbol = "E" }
            };
            Regex rx = new Regex(@"\.0+$|(\.[0-9]*[1-9])0+$");
            lookup.Reverse();
            Lookup? item = lookup.Find((item) => num >= item.Value);
            return (item != null) ? rx.Replace((num / item.Value).ToString($"F{(digits > 0 ? "" : digits)}"), "$1") + item.Symbol : "0";
        }
        public static string GetDeclension(string[] forms, int val)
        {
            int[] cases = new int[] { 2, 0, 1, 1, 1, 2 };
            return forms[(val % 100 > 4 && val % 100 < 20) ? 2 : cases[(val % 10 < 5) ? val % 10 : 5]];
        }
    }

    public class ProperityGet
    {
        public class Properity
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public static Properity[] GetProperites(object element)
        {
            Type type = element.GetType();
            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            Properity[] properites = members.Where(x => x is PropertyInfo).Select(x => new Properity { Name = x.Name, Value = (x as PropertyInfo).GetValue(element, null).ToString() }).ToArray();

            return properites;
        }
    }
}