using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using SharpDX;
using System.Windows.Media.Media3D;
using ExifLib;
using OpenTK.Platform.Windows;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace GygaxCore.DataStructures
{
    public static class NViewMatchLoader
    {
        public static NViewMatch Open(Uri filename, bool loadImages = true, string filter = "")
        {
            return OpenMultiple(filename, loadImages, filter)[0];
        }

        public static NViewMatch[] OpenMultiple(Uri filename, bool loadImages = true, string filter = "")
        {
            List<NViewMatch> models = new List<NViewMatch>();
            
            // Format found at https://code.google.com/archive/p/pais-mvs/wikis/userGuide.wiki
            // and https://github.com/adahbingee/pais-mvs/blob/master/TMVS/io/fileloader.cpp

            StreamReader sr = new StreamReader(filename.LocalPath, ASCIIEncoding.ASCII);
            
            // First line, check if valid
            string s = sr.ReadLine().Trim();

            if (!s.Equals("NVM_V3"))
            {
                throw new FileFormatException(filename, "Not a valid NVM V3 file");
            }

            var end = false;
            var j = 0;

            // k is the id counter
            var k = 0;

            while (!end || !sr.EndOfStream)
            {
                var nvm = new NViewMatch();
                nvm.Filename = filename.LocalPath;

                nvm.TransformationFilename = nvm.Filename.Substring(0, nvm.Filename.Length - 3) + j + ".tfm";
                nvm.Index = j;
                j++;

                // Read empty lines
                do
                {
                    s = sr.ReadLine().Trim();
                } while (s.Equals(""));

                //  Third line gives the number of images
                int numberOfImages = Convert.ToInt32(s);

                if (numberOfImages == 0)
                    end = true;

                // Ok, iterate over images now
                for (int i = 0; i < numberOfImages; i++)
                {
                    s = sr.ReadLine().Trim();

                    var file = s.Split('	');

                    if (file.Length != 2)
                    {
                        end = true;
                        break;
                    }

                    var cp = new CameraPosition
                    {
                        File = (Path.GetDirectoryName(filename.LocalPath) + @"\" + file[0])
                    };

                    double[] coordinates = Array.ConvertAll(file[1].Split(' '), double.Parse);

                    cp.FocalLength = coordinates[0];

                    //cp.Orientation = new Quaternion(coordinates[2], coordinates[3], coordinates[4], coordinates[1]);
                    cp.Orientation = new System.Windows.Media.Media3D.Quaternion(coordinates[2], coordinates[3],
                        coordinates[4], coordinates[1]);

                    cp.CameraCenter = new Vector3D(coordinates[5], coordinates[6], coordinates[7]);
                    cp.RadialDistortion = -coordinates[8];

                    cp.Id = k++;

                    // const for performance reasons
                    // var img = System.Drawing.Image.FromFile(cp.File.LocalPath);
                    //cp.Width = 4592;
                    //cp.Height = 3056;

                    //cp.Width = 7952;
                    //cp.Height = 5304;

                    cp.OpeningAngleDiagonal = 2*
                                              Math.Atan(
                                                  Math.Sqrt(Math.Pow(cp.Width/2.0, 2) + Math.Pow(cp.Height/2.0, 2))/
                                                  cp.FocalLength);

                    //https://msdn.microsoft.com/en-us/library/windows/desktop/ms534416(v=vs.85).aspx

                    //if (cp.File.EndsWith("3490.JPG") || cp.File.EndsWith("3499.JPG"))
                    //if (cp.File.EndsWith("2213.JPG") || cp.File.EndsWith("3129.JPG"))
                    //if (filter == "" || cp.File.EndsWith(filter,true,CultureInfo.CurrentCulture))
                    {
                        if (loadImages)
                        {
                            cp.Image = new Image<Bgr, Byte>(cp.File);
                            cp.Width = cp.Image.Width;
                            cp.Height = cp.Image.Height;
                        }
                        else
                        {
                            using (ExifReader reader = new ExifReader(cp.File))
                            {
                                UInt32 width;
                                reader.GetTagValue(ExifTags.PixelXDimension, out width);

                                UInt32 height;
                                reader.GetTagValue(ExifTags.PixelYDimension, out height);
                                
                                cp.Width = Convert.ToInt32(width);
                                cp.Height = Convert.ToInt32(height);
                            }
                            
                            cp.ImageDiagonal = Math.Sqrt(Math.Pow(cp.Width, 2) + Math.Pow(cp.Height, 2));
                            
                        }

                        nvm.CameraPositions.Add(cp);
                    }
                    

                }

                if (end)
                    break;

                // Line is empty
                do
                {
                    s = sr.ReadLine().Trim();
                } while (s.Equals(""));

                //  Line gives number of patches
                int numberOfPatches = Convert.ToInt32(s);

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
                
                //var maxX = nvm.Patches.Max(p => p.Position.X);
                //var minX = nvm.Patches.Min(p => p.Position.X);

                //var maxY = nvm.Patches.Max(p => p.Position.Y);
                //var minY = nvm.Patches.Min(p => p.Position.Y);

                //var maxZ = nvm.Patches.Max(p => p.Position.Z);
                //var minZ = nvm.Patches.Min(p => p.Position.Z);

                models.Add(nvm);
            }

            if (models.Count == 1)
            {
                models[0].TransformationFilename = models[0].TransformationFilename.Replace(".0.tfm", ".tfm");
            }

            return models.ToArray();
        }

    }

    public class NViewMatch
    {
        public List<CameraPosition> CameraPositions = new List<CameraPosition>();
        public List<Patch> Patches = new List<Patch>();

        public string Filename;
        public string TransformationFilename;
        public int Index;

        public Matrix3D Transform => CoordinateSystem.OpenTransformation(new Uri(TransformationFilename));
    }

    public struct Patch
    {
        public Vector3 Position;

        public SharpDX.Color3 Color;

        public int NumberOfCameras;
    }
}
