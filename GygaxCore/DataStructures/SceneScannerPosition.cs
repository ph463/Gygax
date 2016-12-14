using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;

namespace GygaxCore.DataStructures
{
    public class SceneScannerPosition : Streamable
    {
        public SceneScannerPosition(string file)
        {
            Data = ReadScannerPositions(file);

            Location = file;
            Name = Path.GetFileNameWithoutExtension(Location);
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
                        CameraCenter = new Vector3D(double.Parse(vals[1]), double.Parse(vals[3]), -double.Parse(vals[2])),
                        Name = vals[0].Trim('"'),
                        OpeningAngleVerticalTo = -Math.PI/2.0 * 2.0/3.0
                    };

                    cp.Orientation = CameraPosition.RotationAxis(
                        new Vector3D(
                            double.Parse(vals[4]),
                            double.Parse(vals[6]),
                            -double.Parse(vals[5])),
                        ((double.Parse(vals[7]))*Math.PI/180.0));

                    retList.Add(cp);

                    line = reader.ReadLine();
                } while (!line.StartsWith("}"));
            }

            return retList;
        }
    }
}
