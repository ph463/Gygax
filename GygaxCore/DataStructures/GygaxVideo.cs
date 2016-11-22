using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using GygaxCore.Interfaces;
using NLog;

namespace GygaxCore.DataStructures
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
                if (reader.BaseStream.Length - reader.BaseStream.Position < 12)
                {
                    reader.BaseStream.Position = 0;
                    continue;
                }

                timestamp = reader.ReadInt64();
                length = reader.ReadInt32();

                try
                {
                    if (previousTimestamp != long.MinValue)
                        Thread.Sleep((int) (timestamp - previousTimestamp));
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Warn(e, "Video file corrupt");
                }


                previousTimestamp = timestamp;

                if (reader.BaseStream.Length - reader.BaseStream.Position < length)
                {
                    reader.BaseStream.Position = 0;
                    continue;
                }

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
