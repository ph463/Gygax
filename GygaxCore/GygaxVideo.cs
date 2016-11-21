using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using GygaxCore.Interfaces;

namespace GygaxCore
{
    public class GygaxVideo : Streamable, IVideo
    {
        private bool _stop;

        private long timestamp;
        private int length;

        private BinaryReader reader;

        public GygaxVideo(string filename)
        {
            reader = new BinaryReader(File.Open(filename, FileMode.Open));
            
            var thread = new Thread(WorkThreadFunction);

            Location = filename;

            thread.Name = Location;
            thread.Start();
        }

        private void WorkThreadFunction()
        {
            var previousTimestamp = long.MinValue;

            while (!_stop)
            {
                timestamp = reader.ReadInt64();
                length = reader.ReadInt32();

                if(previousTimestamp != long.MinValue)
                    Thread.Sleep((int)(timestamp - previousTimestamp));

                previousTimestamp = timestamp;

                var bytes = reader.ReadBytes(length);
                var memoryStream = new MemoryStream(bytes);
                var bitmap = new System.Drawing.Bitmap(memoryStream);

                Bitmap masterImage = (Bitmap)bitmap;
                
                CvSource = new Image<Bgr, Byte>(masterImage);
            }
        }

        public override void Close()
        {
            _stop = true;
        }
    }
}
