using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using RemoteAppManager.Core;

namespace RemoteAppManagerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Process[] procesess = Process.GetProcesses();

            AppManagerClient client = new AppManagerClient();
            this.DataContext = client;


            //Process[] processes = Process.GetProcesses();

            //Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(procesess[56].MainModule.FileName);


            //Bitmap bitmap = ico.ToBitmap();
            //IntPtr hBitmap = bitmap.GetHbitmap();

            //ImageSource wpfBitmap =
            //Imaging.CreateBitmapSourceFromHBitmap(
            //          hBitmap, IntPtr.Zero, Int32Rect.Empty,
            //          BitmapSizeOptions.FromEmptyOptions());

            //TestImage.Source = wpfBitmap;
        }
    }
}
