using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GygaxVisu.Dialogs
{
    /// <summary>
    /// Interaction logic for NetworkCameraDialog.xaml
    /// </summary>
    public partial class NetworkCameraDialog : Window
    {
        public bool ok;

        public NetworkCameraDialog()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                IpBlock1.Focus();
            });
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            ok = true;
            this.Close();
        }

        private void ButtonCancle_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
