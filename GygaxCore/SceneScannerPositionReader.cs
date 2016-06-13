using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace GygaxCore
{
    public class SceneScannerPositionReader : Streamable
    {
        public SceneScannerPositionReader(string file)
        {
            Data = ReadScannerPositions(file);
        }

        public static List<CameraPosition> ReadScannerPositions(string file)
        {
            var retList = new List<CameraPosition>();

            string line;
            using (StreamReader reader = new StreamReader(file))
            {
                do
                {
                    line = reader.ReadLine();
                } while (!line.StartsWith("Scans") );

                line = reader.ReadLine();

                do
                {
                    if (line.StartsWith("#"))
                    {
                        line = reader.ReadLine();
                        continue;
                    }

                    var vals = line.Split(' ');
                    var cp = new CameraPosition
                    {
                        Type = CameraPosition.CameraType.Spherical,
                        CameraCenter = new Vector3(float.Parse(vals[1]), float.Parse(vals[3]), -float.Parse(vals[2])),
                        Name = vals[0].Trim('"'),
                        OpeningAngleVerticalTo = -Math.PI/2.0 * 2.0/3.0
                    };
                    
                    cp.Orientation = Quaternion.RotationAxis(
                        new Vector3(
                            float.Parse(vals[4]),
                            float.Parse(vals[6]),
                            -float.Parse(vals[5])),
                        (float)((float.Parse(vals[7])) * Math.PI / 180.0));
                    
                    retList.Add(cp);

                    line = reader.ReadLine();
                } while (!line.StartsWith("}"));
            }

            return retList;
        }

        public override void Close() { }
    }
}
