using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using SharpDX;

namespace GygaxCore
{
    public static class NViewMatchReader
    {
        public static NViewMatch Open(Uri filename, bool loadImages = true, string filter = "")
        {
            // Format found at https://code.google.com/archive/p/pais-mvs/wikis/userGuide.wiki
            // and https://github.com/adahbingee/pais-mvs/blob/master/TMVS/io/fileloader.cpp

            StreamReader sr = new StreamReader(filename.LocalPath, ASCIIEncoding.ASCII);

            var nvm = new NViewMatch();

            // First line, check if valid
            string s = sr.ReadLine().Trim();

            if (!s.Equals("NVM_V3"))
            {
                throw new FileFormatException(filename, "Not a valid NVM V3 file");
            }

            // Second line is empty
            s = sr.ReadLine();

            //  Third line gives the number of images
            int numberOfImages = Convert.ToInt32(sr.ReadLine().Trim());

            // Ok, iterate over images now
            for (int i = 0; i < numberOfImages; i++)
            {
                s = sr.ReadLine().Trim();

                var file = s.Split('	');

                var cp = new CameraPosition
                {
                    File = Path.GetDirectoryName(filename.LocalPath) + @"\" + file[0]
                };

                float[] coordinates = Array.ConvertAll(file[1].Split(' '), float.Parse);

                cp.FocalLength = coordinates[0];
                
                cp.Orientation = new Quaternion(coordinates[2], coordinates[3], coordinates[4], coordinates[1]);
                
                cp.CameraCenter = new Vector3(coordinates[5],coordinates[6], coordinates[7]);
                cp.RadialDistortion = -coordinates[8];

                cp.Id = i;

                cp.Normal = Rotate(cp.Orientation, Vector3.UnitZ);
                
                cp.Basis = new Matrix3x3
                {
                    Row1 = Vector3.Normalize(Rotate(cp.Orientation, Vector3.UnitX)),
                    Row2 = Vector3.Normalize(Rotate(cp.Orientation, Vector3.UnitY)),
                    Row3 = Vector3.Normalize(Rotate(cp.Orientation, Vector3.UnitZ))
                };

                // const for performance reasons
                // var img = System.Drawing.Image.FromFile(cp.File.LocalPath);
                cp.Width = 4592;
                cp.Height = 3056;

                cp.OpeningAngleDiagonal = 2 *
                                  Math.Atan(Math.Sqrt(Math.Pow(cp.Width / 2.0, 2) + Math.Pow(cp.Height / 2.0, 2)) /
                                            cp.FocalLength);

                //if (cp.File.LocalPath.EndsWith("73.JPG") || cp.File.LocalPath.EndsWith("83.JPG") || cp.File.LocalPath.EndsWith("93.JPG") || cp.File.LocalPath.EndsWith("03.JPG"))
                if (filter == "" || cp.File.EndsWith(filter,true,CultureInfo.CurrentCulture))
                {
                    if(loadImages)
                        cp.Image = new Image<Bgr, Byte>(cp.File);

                    nvm.CameraPositions.Add(cp);
                }

            }

            // Line is empty
            s = sr.ReadLine();

            //  Line gives number of patches
            int numberOfPatches = Convert.ToInt32(sr.ReadLine().Trim());

            for (int i = 0; i < numberOfPatches; i++)
            {
                var patch = new Patch();

                patch.Color = new SharpDX.Vector3();
                patch.Position = new Vector3();

                s = sr.ReadLine().Trim();

                float[] values = Array.ConvertAll(s.Split(' '), float.Parse);

                patch.Position.X = values[0];
                patch.Position.Y = values[1];
                patch.Position.Z = values[2];

                patch.Color.Red = values[3];
                patch.Color.Green = values[4];
                patch.Color.Blue = values[5];

                patch.NumberOfCameras = Convert.ToInt32(values[6]);

                nvm.Patches.Add(patch);
            }

            var maxX = nvm.Patches.Max(p => p.Position.X);
            var minX = nvm.Patches.Min(p => p.Position.X);

            var maxY = nvm.Patches.Max(p => p.Position.Y);
            var minY = nvm.Patches.Min(p => p.Position.Y);

            var maxZ = nvm.Patches.Max(p => p.Position.Z);
            var minZ = nvm.Patches.Min(p => p.Position.Z);

            return nvm;
        }

        private static Vector3 Rotate(Quaternion q, Vector3 v)
        {
            var conj = new Quaternion(q.ToArray());
            conj.Conjugate();

            Quaternion rotatedVector = conj * new Quaternion(v, 0) * q;

            return new Vector3(rotatedVector.X, rotatedVector.Y, rotatedVector.Z);
        }
    }

    public class NViewMatch
    {
        public List<CameraPosition> CameraPositions = new List<CameraPosition>();
        public List<Patch> Patches = new List<Patch>();
    }

    public struct Patch
    {
        public Vector3 Position;

        public SharpDX.Color3 Color;

        public int NumberOfCameras;
    }
}
