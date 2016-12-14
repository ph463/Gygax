using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace GygaxCore.DataStructures
{
    public class VideoFromFolder : Streamable, Interfaces.IImage
    {
        private bool _stop;

        private readonly Thread _thread;

        private Uri _folder;
        public Uri Folder
        {
            get { return _folder; }

            set
            {
                _folder = value;
                
                var allowedExtensions = new[] { "jpg", "png", "gif", "bmp"};
                _filePaths = Directory
                    .GetFiles(value.LocalPath)
                    .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                    .ToList();

                _thread.Start();
            }
        }

        private List<string> _filePaths;

        public override void Close()
        {
            _stop = true;
            base.Close();
        }

        public VideoFromFolder()
        {
            _thread = new Thread(WorkThreadFunction);
            _thread.Name = "VideoFromFolder";
        }

        public void WorkThreadFunction()
        {
            int i = 0;

            while (!_stop)
            {
                try
                {
                    CvSource = new Image<Bgr, byte>(_filePaths[i]);
                }
                catch (Exception)
                {
                    // ignored
                }

                i++;
                if (i >= _filePaths.Count) i = 0;

                Thread.Sleep(200);
            }
        }
    }
}
