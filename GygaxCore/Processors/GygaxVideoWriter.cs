using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GygaxCore.DataStructures;

namespace GygaxCore.Processors
{
    public class GygaxVideoWriter : Processor
    {
        public string Filename;

        public GygaxVideoWriter(string filename)
        {
            Filename = filename;
        }

        private BinaryWriter bw;
        private ImageCodecInfo codecInfo;


        private bool _record = false;

        public bool Record
        {
            get { return _record; }
            set
            {
                _record = value;
                OnPropertyChanged("Record");
            }
        }

        public override void Initial()
        {
            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            bw = new BinaryWriter(fs);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();


            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType == "image/jpeg")
                {
                    codecInfo = codecs[i];
                    break;
                }
            }
            
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, QualityLevel);

            // Jpeg image codec
            encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            Update();
        }

        private EncoderParameters encoderParams;

        public int QualityLevel = 80;

        public override void Update()
        {
            if (!Record)
                return;

            if (Source.ImageSource == null)
                return;
            
            var timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            MemoryStream outStream = new MemoryStream();
            Source.CvSource.Bitmap.Save(outStream, codecInfo, encoderParams);
            
            bw.Write(timestamp);
            byte[] bb = outStream.ToArray();
            bw.Write(bb.Length);

            bw.Write(bb);
        }

        public override void Close()
        {
            Record = false;
            bw.Close();
        }
    }
}
