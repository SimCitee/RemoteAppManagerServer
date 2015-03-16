using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Interop;
using System.Xaml;

namespace RemoteAppManager.Core
{
    public enum LogLevels
    {
        INFO = 1,
        WARNING = 2,
        ERROR = 3
    }

    public class Utils
    {
        public static void Log(LogLevels logLevel, String message) {
            String log = String.Empty;

            switch (logLevel) {
                case LogLevels.INFO: log += "[Info]"; break;
                case LogLevels.WARNING: log += "[Warning]"; break;
                case LogLevels.ERROR: log += "[Error]"; break;
                default: log += "[Info]"; break;
            }

            log += " [" + DateTime.Now.ToString() + "] " + message;

            Debug.WriteLine(log);
        }

        public static String BitmapToBase64String(Bitmap bitmap) {
            try {
                ImageConverter converter = new ImageConverter();

                byte[] imageBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
                string base64String = Convert.ToBase64String(imageBytes);

                return base64String;
            }
            catch (Exception e) {
                return String.Empty;
            }
        }

        public static Bitmap Base64StringToBitmap(String base64String) {
            Bitmap bmpReturn = null;

            try {
                byte[] byteBuffer = Convert.FromBase64String(base64String);
                MemoryStream memoryStream = new MemoryStream(byteBuffer);

                memoryStream.Position = 0;

                bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

                memoryStream.Close();
                memoryStream = null;
                byteBuffer = null;
            }
            catch (Exception e) {
            }

            return bmpReturn;
        }

        public static BitmapImage Base64StringToBitmapImage(string base64String) {
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            bitmapImage.Freeze();

            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;

            return bitmapImage;
        }

        public static byte[] ImageToByteArray(Bitmap img) {
            byte[] b;
            using (MemoryStream ms = new MemoryStream()) {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                b = ms.ToArray();
            }
            return b;
        }

        public static Image ByteArrayToImage(byte[] b) {
            using (MemoryStream ms = new MemoryStream(b)) {
                return Bitmap.FromStream(ms);
            }
        }

        public static byte[] Combine(byte[] a, byte[] b) {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        public static ImageSource BitmapToImageSource(Bitmap bitmap) {
            ImageSource imageSource = null;

            try {
                IntPtr hBitmap = bitmap.GetHbitmap();
                imageSource =
                Imaging.CreateBitmapSourceFromHBitmap(
                          hBitmap, IntPtr.Zero, Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
                imageSource.Freeze();
            }
            catch (Exception e) {
                Log(LogLevels.ERROR, e.ToString());
            }

            return imageSource;
        }

        public static String GetTextBetween(String msg, String startDelimiter, String endDelimiter) {
            int indexStart, indexEnd = 0;
            String data = "";

            indexStart = msg.IndexOf(startDelimiter, StringComparison.InvariantCultureIgnoreCase);
            if (indexStart > -1) {
                indexEnd = msg.IndexOf(endDelimiter, indexStart + 1, StringComparison.InvariantCultureIgnoreCase);
                if (indexEnd > -1) {
                    data = msg.Substring(indexStart + startDelimiter.Length, indexEnd - indexStart - startDelimiter.Length);
                }
            }
            return data;
        }
    }
}
