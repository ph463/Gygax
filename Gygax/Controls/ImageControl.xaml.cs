using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using GygaxCore;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Image = GygaxCore.Image;

namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        public static readonly DependencyProperty MyDataContextProperty = DependencyProperty.Register("MyDataContext",
                                            typeof(Object),
                                            typeof(ImageControl),
                                            new PropertyMetadata(DataContextChanged));

        public ImageControl()
        {
            InitializeComponent();
            SetBinding(MyDataContextProperty, new Binding());
        }

        private static void DataContextChanged(
                object sender,
                DependencyPropertyChangedEventArgs e)
        {
            ImageControl myControl = (ImageControl)sender;
            INotifyPropertyChanged person = e.NewValue as INotifyPropertyChanged;
            
        }
        


    }
}
