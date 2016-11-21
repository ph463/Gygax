using System;

namespace GygaxCore.DataStructures
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
                Data = NViewMatchLoader.Open(_file, false);
            }
        }

        private VsfmReconstruction() {}

        public VsfmReconstruction(string filename)
        {
            File = new Uri(filename);
        }

        public static VsfmReconstruction[] OpenMultiple(string filename)
        {
            var a = new System.Collections.Generic.List<VsfmReconstruction>();

            var b = NViewMatchLoader.OpenMultiple(new Uri(filename), false);

            foreach (var nViewMatch in b)
            {
                a.Add(new VsfmReconstruction()
                {
                    _file = new Uri(filename),
                    Data = nViewMatch
                });
            }

            return a.ToArray();
        }

        public override void Close() { }
    }
}
