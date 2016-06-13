using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GygaxCore.Interfaces;
using IImage = Emgu.CV.IImage;

namespace GygaxCore
{
    public abstract class Streamable : IStreamable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private object _data;
        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }
        
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            private set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        private IImage _cvSource;
        public IImage CvSource
        {
            get { return _cvSource; }
            set
            {
                _cvSource = value;
                OnPropertyChanged("CvSource");
                if(_cvSource != null)
                    ImageSource = ToBitmapSource(_cvSource);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.ImageSource
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                );

                bs.Freeze();

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        public abstract void Close();
    }
}
