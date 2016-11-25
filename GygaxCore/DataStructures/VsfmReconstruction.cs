using System;
using System.IO;

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
                Location = value.LocalPath;
                Data = NViewMatchLoader.OpenMultiple(_file, false);
                Name = Path.GetFileNameWithoutExtension(Location);
            }
        }

        private VsfmReconstruction() {}

        public VsfmReconstruction(string filename):base()
        {
            File = new Uri(filename);
        }

        public static VsfmReconstruction[] OpenMultiple(string filename)
        {
            var a = new System.Collections.Generic.List<VsfmReconstruction>();

            var b = NViewMatchLoader.OpenMultiple(new Uri(filename), false);

            var i = 0;

            foreach (var nViewMatch in b)
            {
                a.Add(new VsfmReconstruction()
                {
                    _file = new Uri(filename),
                    Data = nViewMatch,
                    Location = filename,
                    Name = Path.GetFileNameWithoutExtension(filename) + " " + i
                });

                i++;
            }

            return a.ToArray();
        }

        public override void Close() { }
    }
}
