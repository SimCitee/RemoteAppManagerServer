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
using RemoteAppManagerClient.ViewModel;

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

            ClientViewModel client = new ClientViewModel();
            this.DataContext = client;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            SelectAll(sender);
        }

        private void SelectAll(object sender) {
            (sender as TextBox).SelectAll();
        }

        private void TextBox_GotFocus_1(object sender, RoutedEventArgs e) {
            SelectAll(sender);
        }

        private void TextBox_GotFocus_2(object sender, RoutedEventArgs e) {
            SelectAll(sender);
        }

        private void TextBox_GotFocus_3(object sender, RoutedEventArgs e) {
            SelectAll(sender);
        }
    }
}
