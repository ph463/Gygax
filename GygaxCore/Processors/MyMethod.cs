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
    public class MyMethod : Processor
    {
        public override void Initial()
        {
            // Input data at Source.CvSource

            CvSource = Source.CvSource;

            // Write data into CvSource
        }

        public override void Update()
        {
            Initial();
        }
    }
}
