using System;
using System.IO;
using System.Threading;
using Emgu.CV;
using GygaxCore.Interfaces;

namespace GygaxCore.DataStructures
{
    public class Video : Streamable, IVideo
    {
        private readonly Capture _capture = null;

        private bool _stop;

        private double _framesPerSecond;

        public Video(string filename):base()
        {
            Location = filename;
            _capture = new Capture(filename);
            _framesPerSecond = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);

            var thread = new Thread(WorkThreadFunction);
            thread.Name = "Video " + Path.GetFileName(filename);
            thread.Start();
        }

        public override void Close()
        {
            _stop = true;
        }

        private void WorkThreadFunction()
        {
            if (_capture == null) return;

            while (!_stop)
            {
                CvSource = _capture.QueryFrame();

                if(_framesPerSecond > 0)
                    Thread.Sleep(Convert.ToInt32(1000.0/_framesPerSecond));
            }

        }
    }
}
