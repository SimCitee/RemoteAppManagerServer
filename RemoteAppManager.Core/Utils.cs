using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Xaml;

namespace RemoteAppManager.Core
{
    public class Utils
    {
        public enum LogLevels
        {
            INFO = 1,
            WARNING = 2,
            ERROR = 3
        }

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
                return "";
            }
        }

        public static Bitmap Base64StringToBitmap(String base64String) {
            Bitmap bmpReturn = null;

            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);

            memoryStream.Position = 0;

            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;

            return bmpReturn;
        }

        public static BitmapImage Base64StringToBitmapImage(string base64String) {
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.StreamSource = memoryStream;

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
    }
}
