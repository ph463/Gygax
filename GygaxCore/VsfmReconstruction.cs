using System;

namespace GygaxCore
{
    public class VsfmReconstruction : Streamable
    {

        private Uri _file;
        public Uri File
        {
            get { return _file; }

            set
            {
                _file = value;
                Data = NViewMatchReader.Open(_file, false);
            }
        }

        public VsfmReconstruction(string filename)
        {
            File = new Uri(filename);
        }

        public override void Close() { }
    }
}
