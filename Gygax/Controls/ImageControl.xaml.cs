using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GygaxCore.DataStructures;
using GygaxCore.Interfaces;
using Image = GygaxCore.DataStructures.Image;

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
            AddProcessContextMenu();
            SetBinding(MyDataContextProperty, new Binding());
        }

        private void AddProcessContextMenu()
        {
            foreach (var processorType in ProcessorFactory.Instance.Processors)
            {
                var item = new MenuItem
                {
                    Header = processorType
                };

                item.Click += delegate
                {
                    var p = ProcessorFactory.Instance.GetProcessor(processorType);
                    p.Source = (IStreamable)DataContext;
                    ViewModel.AddElementToList(p);
                };

                ProcessorContextMenu.Items.Add(item);
            }
        }

        public event EventHandler Tick;
        //public EventArgs e = null;
        public delegate void EventHandler(object sender, EventArgs e);

        private static void DataContextChanged(
                object sender,
                DependencyPropertyChangedEventArgs e)
        {
            ImageControl myControl = (ImageControl)sender;
            INotifyPropertyChanged person = e.NewValue as INotifyPropertyChanged;
        }


        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {

            //e.GetPosition();
            var pos = e.GetPosition(Image);

            var image = (Image)DataContext;

            System.Drawing.Image theImage = new System.Drawing.Bitmap(image.File.LocalPath);

            // Get the PropertyItems property from image.
            PropertyItem[] propItems = theImage.PropertyItems;

            var t = propItems.Where(p => p.Id == 0x0112).First();

            //https://msdn.microsoft.com/en-us/library/windows/desktop/ms534416(v=vs.85).aspx

            var x = 0.0;
            var y = 0.0;

            switch (t.Value[0])
            {
                case 1:
                    x = pos.X / Image.ActualWidth * image.CvSource.Bitmap.Width;
                    y = pos.Y / Image.ActualHeight * image.CvSource.Bitmap.Height;
                    break;
                case 8:
                    x = image.CvSource.Bitmap.Height - pos.Y / Image.ActualHeight * image.CvSource.Bitmap.Height;
                    y = pos.X / Image.ActualWidth * image.CvSource.Bitmap.Width;
                    break;
                default:
                    throw new NotImplementedException();
            }
            

            var txt = image.Filename + "," + x + "," + y + ",";
            
            Console.WriteLine(txt);
            Clipboard.SetText(txt);
        }
    }
}
