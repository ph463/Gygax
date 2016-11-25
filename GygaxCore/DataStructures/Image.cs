using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace GygaxCore.DataStructures
{
    public class Image: Streamable, Interfaces.IImage
    {
        public double Scale = 1.0;

        private Uri _file;
        public Uri File
        {
            get { return _file; }

            set
            {
                _file = value;
                Image < Bgr, Byte> localImage = new Image<Bgr, Byte>(File.LocalPath);
                CvSource = localImage.Resize((int)(localImage.Width*Scale), (int)(localImage.Height * Scale), Inter.Linear);
                localImage.Dispose();
            }
        }

        public Image(string filename):this(filename, 1.0)
        {
        }

        public Image(string filename, double scale)
        {
            Scale = scale;
            Location = filename;
            File = new Uri(Location);
        }

        public override void Close() { }
    }
}
