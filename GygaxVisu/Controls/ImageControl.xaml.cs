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
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using GygaxCore.DataStructures;
using GygaxCore.Interfaces;
using GygaxCore.Processors;
using NLog;
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

            SizeChanged += OnSizeChanged;

            InitializeContextMenu();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (((ImageControl)sender).ActualWidth > 300)
            {
                ContextMenu = null;
            }
            else
            {
                InitializeContextMenu();
            }
        }

        private void InitializeContextMenu()
        {
            ContextMenu = new ContextMenu();

            var hideItem = new MenuItem()
            {
                Header = "Minimise"
            };

            hideItem.Click += delegate(object sender, RoutedEventArgs args)
            {
                _controlHidden = !_controlHidden;

                if (_controlHidden)
                {
                    Image.Visibility = Visibility.Collapsed;
                    Label.Visibility = Visibility.Visible;
                }
                else
                {
                    Image.Visibility = Visibility.Visible;
                    Label.Visibility = Visibility.Collapsed;
                }
                hideItem.IsChecked = _controlHidden;
            };

            ContextMenu.Items.Add(hideItem);

            var RecordItem = new MenuItem
            {
                Header = "Save"
            };

            RecordItem.Click += delegate(object sender, RoutedEventArgs args)
            {
                Save((IStreamable)DataContext);
            };

            ContextMenu.Items.Add(RecordItem);

            var InfoItem = new MenuItem
            {
                Header = "Info"
            };

            ContextMenu.Opened += delegate(object sender, RoutedEventArgs args)
            {
                InfoItem.Items.Clear();

                try
                {
                    InfoItem.Items.Add(new MenuItem
                    {
                        Header = "Name: " + ((IStreamable)DataContext).Name
                    });

                    InfoItem.Items.Add(new MenuItem
                    {
                        Header = "Location: " + ((IStreamable)DataContext).Location
                    });
                }
                catch (Exception) { }

            };

            ContextMenu.Items.Add(InfoItem);

            var CloseItem = new MenuItem
            {
                Header = "Close"
            };

            CloseItem.Click += delegate (object sender, RoutedEventArgs args)
            {
                ((IStreamable)DataContext).Close();
            };

            ContextMenu.Items.Add(CloseItem);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "Data":
                case "DataContext":
                    break;
            }
        }

        private GygaxVideoWriter videoWriter;

        private void Save(IStreamable sender)
        {
            if (sender is IImage)
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.FileName = "*";
                saveFileDialog.DefaultExt = "jpg";
                saveFileDialog.ValidateNames = true;

                saveFileDialog.Filter = "Image File (.jpg)|*.jpg";

                DialogResult result = saveFileDialog.ShowDialog();

                if (!(result == DialogResult.OK)) // Test result.
                {
                    return;
                }
                
                sender.Save(saveFileDialog.FileName);
            }
            else if (sender is IVideo || sender is ICamera)
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

                    RecordingButton.Visibility = Visibility.Visible;
                    StopRecordingButton.Visibility = Visibility.Visible;
                }
            }
        }

        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ImageControl myControl = (ImageControl)sender;

            if (e.NewValue == null)
                return;

            myControl.OnPropertyChanged(e.NewValue, new PropertyChangedEventArgs("DataContext"));
        }

        private void RecordingButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (videoWriter == null)
                return;

            videoWriter.Record = !videoWriter.Record;

            if (videoWriter.Record)
            {
                LogManager.GetCurrentClassLogger().Info("Recording started ("+videoWriter.Filename+")");
            }
            else
            {
                LogManager.GetCurrentClassLogger().Info("Recording paused (" + videoWriter.Filename+")");
            }
        }

        private void StopRecordingButton_OnClick(object sender, RoutedEventArgs e)
        {
            videoWriter.Close();

            LogManager.GetCurrentClassLogger().Info("Recording finished (" + videoWriter.Filename + ")");

            videoWriter = null;

            RecordingButton.Visibility = Visibility.Collapsed;
            StopRecordingButton.Visibility = Visibility.Collapsed;
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
