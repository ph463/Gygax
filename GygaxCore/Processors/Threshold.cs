using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Emgu.CV.Structure;
using GygaxCore.DataStructures;

namespace GygaxCore.Processors
{
    public class Threshold : Processor
    {
        public override void Initial()
        {
            if (Source.CvSource is Emgu.CV.Image<Bgr, byte>)
            {
                CvSource = ((Emgu.CV.Image<Bgr, byte>) Source.CvSource).ThresholdBinary(new Bgr(128,128,128), new Bgr(255, 255, 255));
            }
            else if (Source.CvSource is Emgu.CV.Image<Bgr, Single>)
            {
                CvSource = ((Emgu.CV.Image<Bgr, Single>)Source.CvSource).ThresholdBinary(new Bgr(128, 128, 128), new Bgr(255, 255, 255));
            }
            else if (Source.CvSource is Emgu.CV.Mat)
            {
                CvSource = ((Emgu.CV.Mat)Source.CvSource).ToImage<Bgr, byte>().ThresholdBinary(new Bgr(128, 128, 128), new Bgr(255, 255, 255));
            }
        }

        public override void Update()
        {
            Initial();
        }
    }
}
