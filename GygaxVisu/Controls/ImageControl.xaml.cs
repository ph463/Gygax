using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using GygaxCore.DataStructures;
using GygaxCore.Interfaces;
using GygaxCore.Processors;
using Binding = System.Windows.Data.Binding;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;

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

        private bool _controlHidden = false;

        public ImageControl()
        {
            InitializeComponent();
            SetBinding(MyDataContextProperty, new Binding());
            
            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            ContextMenu = new ContextMenu();

            var hideItem = new MenuItem()
            {
                Header = "Hide"
            };

            hideItem.Click += delegate (object sender, RoutedEventArgs args)
            {
                _controlHidden = !_controlHidden;

                if (_controlHidden)
                    Height = 10;
                else
                {
                    Height = Double.NaN; // Auto height
                }
                hideItem.IsChecked = _controlHidden;
            };

            ContextMenu.Items.Add(hideItem);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "Data":
                case "DataContext":

                    AddContextMenuRecord(sender);
                    break;
            }
        }

        private GygaxVideoWriter videoWriter;

        private void AddContextMenuRecord(object sender)
        {
            if (sender is IVideo || sender is ICamera)
            {
                var item = new MenuItem
                {
                    Header = "Recording",
                };

                ContextMenu.Items.Add(item);

                var startItem = new MenuItem
                {
                    Header = "Start recording",
                };

                startItem.Click += delegate(object o, RoutedEventArgs args)
                {
                    if (videoWriter == null)
                    {
                        var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                        saveFileDialog.FileName = "*";
                        saveFileDialog.DefaultExt = "gyg";
                        saveFileDialog.ValidateNames = true;

                        saveFileDialog.Filter = "Gygax Video (.gyg)|*.gyg";

                        DialogResult result = saveFileDialog.ShowDialog();

                        if (!(result == DialogResult.OK)) // Test result.
                        {
                            return;
                        }

                        videoWriter = new GygaxVideoWriter(saveFileDialog.FileName);
                        videoWriter.Source = (IStreamable)sender;
                    }

                    videoWriter.Record = !videoWriter.Record;
                    startItem.IsChecked = videoWriter.Record;

                    if (videoWriter.Record)
                    {
                        startItem.Header = "Pause recording";
                        RecordingFrame.BorderThickness = new Thickness(5);
                    }
                    else
                    {
                        startItem.Header = "Continue recording";
                        RecordingFrame.BorderThickness = new Thickness(0);
                    }
                };


                item.Items.Add(startItem);

                var stopItem = new MenuItem
                {
                    Header = "Stop recording",
                };

                stopItem.Click += delegate(object o, RoutedEventArgs args)
                {
                    videoWriter.Close();
                    videoWriter = null;
                    startItem.IsChecked = false;
                    startItem.Header = "Start recording";
                    RecordingFrame.BorderThickness = new Thickness(0);
                };

                item.Items.Add(stopItem);
            }
        }

        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ImageControl myControl = (ImageControl)sender;

            if (e.NewValue == null)
                return;

            myControl.OnPropertyChanged(e.NewValue, new PropertyChangedEventArgs("DataContext"));
        }

        //private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    //e.GetPosition();
        //    var pos = e.GetPosition(Image);

        //    var image = (Image)DataContext;

        //    System.Drawing.Image theImage = new System.Drawing.Bitmap(image.File.LocalPath);

        //    // Get the PropertyItems property from image.
        //    PropertyItem[] propItems = theImage.PropertyItems;

        //    var t = propItems.Where(p => p.Id == 0x0112).First();

        //    //https://msdn.microsoft.com/en-us/library/windows/desktop/ms534416(v=vs.85).aspx

        //    var x = 0.0;
        //    var y = 0.0;

        //    switch (t.Value[0])
        //    {
        //        case 1:
        //            x = pos.X / Image.ActualWidth * image.CvSource.Bitmap.Width;
        //            y = pos.Y / Image.ActualHeight * image.CvSource.Bitmap.Height;
        //            break;
        //        case 8:
        //            x = image.CvSource.Bitmap.Height - pos.Y / Image.ActualHeight * image.CvSource.Bitmap.Height;
        //            y = pos.X / Image.ActualWidth * image.CvSource.Bitmap.Width;
        //            break;
        //        default:
        //            throw new NotImplementedException();
        //    }
            

        //    var txt = image.Filename + "," + x + "," + y + ",";
            
        //    Console.WriteLine(txt);
        //    Clipboard.SetText(txt);
        //}
    }
}
