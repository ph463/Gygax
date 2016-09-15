using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using GygaxCore.Interfaces;

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
                Filename = Path.GetFileName(File.LocalPath);
            }
        }

        public string Filename { get; private set; }

        public Image(string filename)
        {
            File = new Uri(filename);
        }

        public override void Close() { }
    }
}
