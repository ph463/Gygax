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
    public class EdgeDetector : Processor
    {
        public override void Initial()
        {
            if (Source.CvSource is Emgu.CV.Image<Bgr, byte>)
            {
                CvSource = ((Emgu.CV.Image<Bgr, byte>) Source.CvSource).Sobel(1, 0, 5);
            }
            else if (Source.CvSource is Emgu.CV.Image<Bgr, Single>)
            {
                CvSource = ((Emgu.CV.Image<Bgr, Single>)Source.CvSource).Sobel(1, 0, 5);
            }
            else if (Source.CvSource is Emgu.CV.Mat)
            {
                CvSource = ((Emgu.CV.Mat)Source.CvSource).ToImage<Bgr, byte>().Sobel(1, 0, 5);
            }
        }

        public override void Update()
        {
            Initial();
        }
    }
}
