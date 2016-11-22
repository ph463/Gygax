using System;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace GygaxCore.DataStructures
{
    public class Image: Streamable, Interfaces.IImage
    {
        private Uri _file;
        public Uri File
        {
            get { return _file; }

            set
            {
                _file = value;
                CvSource = new Image<Bgr, Byte>(File.LocalPath);
            }
        }

        //public string Filename { get; private set; }

        public Image(string filename)
        {
            Location = filename;
            File = new Uri(Location);
        }

        public override void Close() { }
    }
}
