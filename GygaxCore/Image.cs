using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using IImage = GygaxCore.Interfaces.IImage;

namespace GygaxCore
{
    public class Image: Streamable, IImage
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

        public Image(string filename)
        {
            File = new Uri(filename);
        }

        public override void Close() { }
    }
}
