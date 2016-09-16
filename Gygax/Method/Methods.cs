using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Accord.Statistics;
using Emgu.CV;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using GygaxCore.DataStructures;
using GygaxCore.Ifc;
using GygaxVisu.Visualizer;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.VisualBasic.FileIO;
using OpenTK.Graphics.OpenGL;
using PclWrapper;
using SharpDX;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using ProtoBuf;

namespace GygaxVisu.Method
{
    public class Methods
    {
        public void Icp(bool doIcp)
        {
            var res = new OpenFileDialog
            { Filter = "Pointcloud file|*.pcd;..." };

            if (res.ShowDialog() != DialogResult.OK)
            { return; }

            var res2 = new OpenFileDialog
            { Filter = "NVM file|*.nvm;..." };

            if (res2.ShowDialog() != DialogResult.OK)
            { return; }
            
            //var baseDir = @"Z:\06. Data\Bridges\Philipp";
            //var bridgeFolder = "Bridge 1";
            //var elementFolder = "102COLUM";
            //var subDir = @"\GainCompensation\GainCompensation";
            ////var subDir = "";
            
            //var vsfm = VsfmReconstruction.OpenMultiple(baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + subDir + @"\sparse.nvm");
            //var pcl = new Pointcloud(baseDir + @"\" + bridgeFolder + @"\IFC\less-subsample_01.pcd");
            //var correspondencesFilenameBase = baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + subDir + @"\Correspondences.csv";
            //var manualTransformFilenameBase = baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + subDir + @"\Transform.csv";
            //var finalTransformFilenameBase = baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + subDir + @"\sparse.tfm";

            
            var vsfm = VsfmReconstruction.OpenMultiple(res2.FileName);
            var correspondencesFilenameBase = Path.GetDirectoryName(res2.FileName) + @"\Correspondences.csv";
            var manualTransformFilenameBase = Path.GetDirectoryName(res2.FileName) + @"\Transform.csv";
            var finalTransformFilenameBase = Path.GetDirectoryName(res2.FileName) + @"\sparse.tfm";

            var correspondencesFilename = correspondencesFilenameBase;
            //var manualTransformFilename = manualTransformFilenameBase;
            var finalTransformFilename = finalTransformFilenameBase;

            var pcl = new Pointcloud(res.FileName);
            pcl.PropertyChanged += delegate(object o, PropertyChangedEventArgs args)
            {
                for (int j = 0; j < vsfm.Length; j++)
                {
                    if (vsfm.Length > 1)
                    {
                        correspondencesFilename = correspondencesFilenameBase.Replace(".csv", "." + j + ".csv");
                        //manualTransformFilename = manualTransformFilenameBase.Replace(".csv", "." + j + ".csv");
                        finalTransformFilename = finalTransformFilenameBase.Replace(".tfm", "." + j + ".tfm");
                    }

                    if (!File.Exists(correspondencesFilename)) continue;

                    var a = (PointGeometry3D) pcl.Data;
                    int i = 0;

                    Points[] ap = new Points[a.Points.Count()];
                    foreach (var point in a.Points)
                    {
                        ap[i++] = new Points {x = point.P0.X, y = point.P0.Y, z = point.P0.Z};
                    }

                    var b = (NViewMatch) vsfm[j].Data;
                    i = 0;

                    var cs = new CoordinateSystem();
                    cs.LoadCorrespondences(correspondencesFilename);
                    cs.CalculateHomography();
                    //cs.SaveTransform(manualTransformFilename);

                    var tr = new Transform3DGroup();

                    tr.Children.Add(
                        new RotateTransform3D(new QuaternionRotation3D(new System.Windows.Media.Media3D.Quaternion()
                        {
                            X = cs.Rotation.X,
                            Y = cs.Rotation.Y,
                            Z = cs.Rotation.Z,
                            W = cs.Rotation.W
                        })));

                    tr.Children.Add(new TranslateTransform3D(new Vector3D(
                        cs.Translation.X,
                        cs.Translation.Y,
                        cs.Translation.Z
                        )));

                    tr.Children.Add(new TranslateTransform3D(
                        -cs.Centroid.ParentCoordinateSystem.X,
                        -cs.Centroid.ParentCoordinateSystem.Y,
                        -cs.Centroid.ParentCoordinateSystem.Z
                        ));

                    tr.Children.Add(new ScaleTransform3D(1/cs.Scaling, 1/cs.Scaling, 1/cs.Scaling));

                    tr.Children.Add(new TranslateTransform3D(
                        cs.Centroid.ParentCoordinateSystem.X,
                        cs.Centroid.ParentCoordinateSystem.Y,
                        cs.Centroid.ParentCoordinateSystem.Z
                        ));

                    if (doIcp)
                    {
                        Points[] bp = new Points[b.Patches.Count()];
                        foreach (var patch in b.Patches)
                        {
                            var n = cs.ConvertToParentCoordinate(patch.Position);
                            bp[i++] = new Points {x = n.X, y = n.Y, z = n.Z};
                        }

                        float[] t = new float[16];

                        var lib = new PCD();
                        lib.Process4(ap, bp, t);

                        tr.Children.Add(new MatrixTransform3D(new Matrix3D(
                            t[0], t[1], t[2], t[3],
                            t[4], t[5], t[6], t[7],
                            t[8], t[9], t[10], t[11],
                            t[12], t[13], t[14], t[15]
                        )));
                    }

                    CoordinateSystem.WriteTransformation(new Uri(finalTransformFilename), tr.Value);
                }
            };
        }

        public static void ExportToObject()
        {
            var ifcFile = @"C:\Users\Philipp\Documents\CIT\Bridge 1\IFC\Bridge1_v3.ifc";

            var f = new IfcViewerWrapper();
            f.OpenIFCFile(ifcFile);

            //var p = IfcVisualizer.GetItems(f, false, new[] {"Concrete-Round-Column:Pier:251769"});
            var p = IfcVisualizer.GetItems(f, false);

            var objHandler = new ObjHandler();
            objHandler.Export(p, @"C:\Users\Philipp\Desktop\test2");

        }

        public static void CheckFrustum(ref List<CameraPosition> cameraPositions,ref List<Triangle> triangles, out Dictionary<Triangle, List<CameraPosition>> dict, out Dictionary<CameraPosition, List<Triangle>> tris)
        {

            dict = new Dictionary<Triangle, List<CameraPosition>>();
            tris = new Dictionary<CameraPosition, List<Triangle>>();


            foreach (var cameraPosition in cameraPositions)
            {
                var b = BoundingFrustum.FromCamera(cameraPosition.CameraCenter.ToVector3(),
                    CameraPosition.Rotate(cameraPosition.Orientation, new Vector3D(0, 0, 1)).ToVector3(),
                    CameraPosition.Rotate(cameraPosition.Orientation, new Vector3D(0, 1, 0)).ToVector3(),
                    (float)cameraPosition.OpeningAngleDiagonal, 0,100,1);

                foreach (var triangle in triangles)
                {
                    //var corners = new Vector3[3];
                    for (int i = 0; i < 3; i++)
                    {
                        //corners[i] = triangle.Corners[i].Coordinates3DFloat;
                        var ct = b.Contains(triangle.Corners[i].Coordinates3DFloat);

                        if (ct != ContainmentType.Disjoint)
                        {
                            if (!dict.ContainsKey(triangle))
                                dict.Add(triangle, new List<CameraPosition>());

                            dict[triangle].Add(cameraPosition);

                            if (!tris.ContainsKey(cameraPosition))
                                tris.Add(cameraPosition, new List<Triangle>());

                            tris[cameraPosition].Add(triangle);

                            break;
                        }
                    } 
                }
            }


            //return dict;

        }

        public static void CalculateOneTexture(
            string nvmFile, string ifcFile, string textureDirectory, bool generateTextureFile = true, bool generateMaskFile = false)
        {
            var nvm = NViewMatchLoader.OpenMultiple(new Uri(nvmFile), false,"", false);

            var cameraPositions = new List<CameraPosition>();

            foreach (var nViewMatch in nvm)
            {
                if (!File.Exists(nViewMatch.TransformationFilename)) continue;

                var transform =
                               CoordinateSystem.OpenTransformation(
                                   new Uri(nViewMatch.TransformationFilename));

                foreach (var cameraPosition in nViewMatch.CameraPositions)
                {
                    var cp = cameraPosition;

                    cameraPosition.Transform(transform);

                    cameraPositions.Add(cp);
                }
            }

            var f = new IfcViewerWrapper();
            f.OpenIFCFile(ifcFile);

            var triangles = new List<Triangle>();

            //var p = IfcVisualizer.GetItems(f, false, new []{ "Concrete-Round-Column:Pier:251769" });
            var p = IfcVisualizer.GetItems(f, false);


            //Viewport = new Viewport3DX();

            foreach (var item in p)
            {
                item.Mapper = new UvMapper((MeshGeometry3D)item.Geometry, p, item.IfcName, false)
                {
                    TextureFilename = textureDirectory + IfcViewerWrapper.GetValidPathName(item.IfcName) + ".jpg",
                    TextureHeight = item.TextureHeight,
                    TextureWidth = item.TextureWidth
                };

                item.Mapper.GenerateIndexes();

                triangles.AddRange(item.Mapper.Triangles);
                

                models.Add(item);

                //Viewport.Items.Add(p);
            }

            Dictionary<Triangle, List<CameraPosition>> dict;
            Dictionary<CameraPosition, List<Triangle>> tris;

            CheckFrustum(ref cameraPositions, ref triangles, out dict, out tris);

            var listDoGenerate =
                p.Where(q => q.IfcName.Contains("Column")).Where(q => !q.IfcName.Contains("251769")).ToList();
            

            foreach (var item in listDoGenerate)
            {
                item.Mapper.GenerateSurfaceImageryFromCameraList(ref dict, ref tris, cameraPositions, item.Mapper.Triangles, generateTextureFile, generateMaskFile);
            }
        }

        //public static Viewport3DX Viewport;
        public static List<MeshGeometryModel3D> models = new List<MeshGeometryModel3D>();

        public static void CalculateTexture(
            string nvmFile, string ifcFile, string textureDirectory, string[] elementName, bool generateTextureFile = true, bool generateMaskFile = false)
        {
            var nvm = NViewMatchLoader.OpenMultiple(new Uri(nvmFile));

            var cameraPositions = new List<CameraPosition>();

            foreach (var nViewMatch in nvm)
            {
                if (!File.Exists(nViewMatch.TransformationFilename)) continue;

                var transform =
                               CoordinateSystem.OpenTransformation(
                                   new Uri(nViewMatch.TransformationFilename));

                foreach (var cameraPosition in nViewMatch.CameraPositions)
                {
                    var cp = cameraPosition;

                    cameraPosition.Transform(transform);

                    cameraPositions.Add(cp);
                }
            }

            var f = new IfcViewerWrapper();
            f.OpenIFCFile(ifcFile);

            var triangles = new List<Triangle>();

            var p = IfcVisualizer.GetItems(f, false, elementName);

            foreach (var item in p)
            {
                item.Mapper = new UvMapper((MeshGeometry3D)item.Geometry, p, item.IfcName, false)
                {
                    TextureFilename = textureDirectory + IfcViewerWrapper.GetValidPathName(item.IfcName) + ".jpg",
                    TextureHeight = item.TextureHeight,
                    TextureWidth = item.TextureWidth
                };

                item.Mapper.GenerateIndexes();

                triangles.AddRange(item.Mapper.Triangles);
            }

            foreach (var triangle in triangles)
            {
                triangle.CalculateVisibility(cameraPositions, triangles);
            }

            foreach (var item in p)
            {
                //item.Mapper.GenerateSurfaceImageryFromCameraList(cameraPositions, item.Mapper.Triangles, generateTextureFile, generateMaskFile);
                throw new NotImplementedException();
            }
        }

        private void IterateDirectory()
        {
            var root = @"Z:\06. Data\Bridges\Philipp\Bridge 1\Sorted";

            foreach (var dir in Directory.GetDirectories(root))
            {
                var subdir = @"\GainCompensation\GainCompensation";

                foreach (var file in Directory.GetFiles(dir + subdir, "*.jpg"))
                {

                    var gainCompensatedFile = Path.GetDirectoryName(file) + @"\..\..\..\..\JustImages\" + Path.GetFileName(file).Replace(".ppm.jpg", ".JPG");

                    gainCompensatedFile = Path.GetFullPath(gainCompensatedFile);


                    if(!_fileAssignment.ContainsKey(gainCompensatedFile))
                        _fileAssignment.Add(gainCompensatedFile, file);
                }
            }

        }

        private Dictionary<string, string> _fileAssignment = new Dictionary<string, string>();


        public void DrawTexture(string binfilename)
        {
            string textureFilename = binfilename.Replace(".0.bin", ".jpg").Replace(".bin", ".jpg");

            IterateDirectory();


            var recon = ReconstructionContainer.Read(binfilename);


            Image<Bgr, Byte> image = new Image<Bgr, Byte>(recon.Width, recon.Height);

            var filenames = recon.Points.Select(c => c.Filename).Where(d => d != null).Distinct();

            Parallel.ForEach(filenames, filename =>
            {
                var path = Path.GetFullPath(filename);

                var img = new Image<Bgr, Byte>(_fileAssignment[path]);

                for (int i = 0; i < recon.Points.Length; i++)
                {

                    if (filename != recon.Points[i].Filename)
                        continue;

                    var textureX = i % recon.Width;
                    var textureY = i / recon.Width;

                    if (img.Height > img.Width)
                        image[textureY, textureX] = img[recon.Points[i].ImageX, recon.Points[i].ImageY];
                    else
                        image[textureY, textureX] = img[recon.Points[i].ImageY, recon.Points[i].ImageX];

                }

                img.Dispose();
            });

            image.Save(textureFilename);
        }

        public void GenerateAllTextures()
        {
            var baseDir = @"Z:\06. Data\Bridges\Philipp";
            var bridgeFolder = "Bridge 1";
            var subDir = @"\GainCompensation\GainCompensation";
            //var subDir = "";

            string ifcFile = baseDir + @"\" + bridgeFolder + @"\IFC\Bridge1_v3.ifc";
            string textureDirectory = baseDir + @"\" + bridgeFolder + @"\Textures\";

            var f = Directory.GetFiles(baseDir + @"\" + bridgeFolder + @"\Sorted\");

            foreach (var s in f)
            {       
                string mainFile = s + subDir + @"\sparse.nvm";
                string[] elementName =
                    File.ReadAllLines(s + @"\ifcElement.txt");

                CalculateTexture(mainFile, ifcFile, textureDirectory, elementName);
            }
        }

        public void CalculateTexture(bool generateTextureFile = true, bool generateMaskFile = false)
        {
            var baseDir = @"Z:\06. Data\Bridges\Philipp";
            var bridgeFolder = "Bridge 1";
            var elementFolder = "102COLUM";
            var subDir = @"\GainCompensation\GainCompensation";
            //var subDir = "";

            string ifcFile = baseDir + @"\" + bridgeFolder + @"\IFC\Bridge1_v3.ifc";
            string mainFile = baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + subDir + @"\sparse.nvm";
            string textureDirectory = baseDir + @"\" + bridgeFolder + @"\Textures\";
            string[] elementName =
                File.ReadAllLines(baseDir + @"\" + bridgeFolder + @"\Sorted\" + elementFolder + @"\ifcElement.txt");

            CalculateTexture(mainFile, ifcFile, textureDirectory, elementName, generateTextureFile, generateMaskFile);
        }

        public void ExtractFromImageCoordinates()
        {
            var res = new OpenFileDialog { Filter = "NView Match|*.nvm;..." };

            if (res.ShowDialog() != DialogResult.OK)
                return;
            
            var model =
                NViewMatchLoader.OpenMultiple(
                    new Uri(res.FileName),
                    false);

            if (model.Length == 1)
            {
                using (
                    TextFieldParser parser =
                        new TextFieldParser(Path.GetDirectoryName(res.FileName) + @"\ManualPicks.csv"))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    System.IO.StreamWriter file = new System.IO.StreamWriter(Path.GetDirectoryName(res.FileName) + @"\Correspondences.csv");

                    while (!parser.EndOfData)
                    {
                        //Processing row
                        string[] fields = parser.ReadFields();

                        if (fields.Count() != 9)
                            return;

                        var r = GetManualPickFromTwoImages(model.First(), fields[0], Double.Parse(fields[1]),
                            Double.Parse(fields[2]), fields[3], Double.Parse(fields[4]), Double.Parse(fields[5]));

                        Console.WriteLine(r.X + "," + r.Y + "," + r.Z);
                        file.WriteLine(r.X + "," + r.Y + "," + r.Z + "," + fields[6] + "," + fields[7] + "," + fields[8]);
                    }

                    file.Close();
                }
            }
            else
            {
                for (int i = 0; i < model.Length; i++)
                {
                    if (!File.Exists(Path.GetDirectoryName(res.FileName) + @"\ManualPicks." + i + ".csv"))
                        continue;

                    using (
                    TextFieldParser parser =
                        new TextFieldParser(Path.GetDirectoryName(res.FileName) + @"\ManualPicks." + i +".csv"))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        System.IO.StreamWriter file = new System.IO.StreamWriter(Path.GetDirectoryName(res.FileName) + @"\Correspondences." + i + ".csv");

                        while (!parser.EndOfData)
                        {
                            //Processing row
                            string[] fields = parser.ReadFields();

                            if (fields.Count() != 9)
                                return;

                            var r = GetManualPickFromTwoImages(model[i], fields[0], Double.Parse(fields[1]),
                                Double.Parse(fields[2]), fields[3], Double.Parse(fields[4]), Double.Parse(fields[5]));

                            Console.WriteLine(r.X + "," + r.Y + "," + r.Z);
                            file.WriteLine(r.X + "," + r.Y + "," + r.Z + "," + fields[6] + "," + fields[7] + "," + fields[8]);
                        }

                        file.Close();
                    }
                }
            }
        }

        public Vector3 GetManualPickFromTwoImages(NViewMatch model, string filename1, double x1, double y1, string filename2, double x2, double y2)
        {
            var ray1 = ExtractFromImageCoordinates(model, x1, y1, filename1);
            
            var ray2 = ExtractFromImageCoordinates(model, x2, y2, filename2);

            var p1 = GetClosestPoint(ray1, ray2);
            var p2 = GetClosestPoint(ray2, ray1);

            var p = p1 + (p2 - p1) / 2.0f;

            var error = (p2 - p1).Length();

            //Console.WriteLine("Error: " + error);

            return p;
        }

        private Vector3 GetClosestPoint(Ray ray1, Ray ray2)
        {
            Vector3 intersection;

            var normal = Vector3.Cross(ray1.Direction, ray2.Direction);
            normal.Normalize();
            
            var plane = new Plane(ray2.Position, normal);

            var projection = new Ray(ray1.Position, normal);

            if (!SharpDX.Collision.RayIntersectsPlane(ref projection, ref plane, out intersection))
            {
                projection = new Ray(ray1.Position, -normal);
                if (!SharpDX.Collision.RayIntersectsPlane(ref projection, ref plane, out intersection))
                {
                    throw new Exception("Houston, we have a problem");
                }
            }

            var p3 = new Ray(intersection, ray1.Direction);

            if (!RayIntersectsRay(ref p3, ref ray2, out intersection))
            {
                p3 = new Ray(intersection, -ray1.Direction);
                if (!RayIntersectsRay(ref p3, ref ray2, out intersection))
                {
                    throw new Exception("Houston, we have a problem");
                }
            }

            return intersection;
        }

        public static bool RayIntersectsRay(ref Ray ray1, ref Ray ray2, out Vector3 point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780

            Vector3 cross;

            Vector3.Cross(ref ray1.Direction, ref ray2.Direction, out cross);
            float denominator = cross.Length();

            //Lines are parallel.
            if (MathUtil.IsZero(denominator))
            {
                //Lines are parallel and on top of each other.
                if (MathUtil.NearEqual(ray2.Position.X, ray1.Position.X) &&
                    MathUtil.NearEqual(ray2.Position.Y, ray1.Position.Y) &&
                    MathUtil.NearEqual(ray2.Position.Z, ray1.Position.Z))
                {
                    point = Vector3.Zero;
                    return true;
                }
            }

            denominator = denominator * denominator;

            //3x3 matrix for the first ray.
            float m11 = ray2.Position.X - ray1.Position.X;
            float m12 = ray2.Position.Y - ray1.Position.Y;
            float m13 = ray2.Position.Z - ray1.Position.Z;
            float m21 = ray2.Direction.X;
            float m22 = ray2.Direction.Y;
            float m23 = ray2.Direction.Z;
            float m31 = cross.X;
            float m32 = cross.Y;
            float m33 = cross.Z;

            //Determinant of first matrix.
            float dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //3x3 matrix for the second ray.
            m21 = ray1.Direction.X;
            m22 = ray1.Direction.Y;
            m23 = ray1.Direction.Z;

            //Determinant of the second matrix.
            float dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //t values of the point of intersection.
            float s = dets / denominator;
            float t = dett / denominator;

            //The points of intersection.
            Vector3 point1 = ray1.Position + (s * ray1.Direction);
            Vector3 point2 = ray2.Position + (t * ray2.Direction);

            //If the points are not equal, no intersection has occurred.
            //if (!MathUtil.NearEqual(point2.X, point1.X) ||
            //    !MathUtil.NearEqual(point2.Y, point1.Y) ||
            //    !MathUtil.NearEqual(point2.Z, point1.Z))
            //{
            //    point = Vector3.Zero;
            //    return false;
            //}

            point = (point1+point2)/2;
            return true;
        }

        public static Ray ExtractFromImageCoordinates(NViewMatch model, double x, double y, string filename)
        {
            var cameraPosition = model.CameraPositions.First(c => c.File.EndsWith(filename));

            var xToImageCenter = x - cameraPosition.Width / 2.0;
            var yToImageCenter = y - cameraPosition.Height / 2.0;

            var basis = cameraPosition.Basis;
            basis.Invert();

            var imagePlane = new Plane((cameraPosition.CameraCenter + cameraPosition.Normal * cameraPosition.FocalLength).ToVector3(), cameraPosition.Normal.ToVector3());
            var rayCenter = new Ray(cameraPosition.CameraCenter.ToVector3(), cameraPosition.Normal.ToVector3());

            Vector3 intersectionCenter;
            imagePlane.Intersects(ref rayCenter, out intersectionCenter);
            
            var point = new Vector3D(intersectionCenter.X, intersectionCenter.Y, intersectionCenter.Z);

            var vector = new Vector3D(xToImageCenter, yToImageCenter, 0);
            var vec = CameraPosition.Rotate(cameraPosition.Orientation, vector);
            
            point = point + vec;

            var direction = point - cameraPosition.CameraCenter;
            direction.Normalize();

            return new Ray(cameraPosition.CameraCenter.ToVector3(), direction.ToVector3());
        }

        public void MethodA()
        {
            Console.WriteLine("this is method A");
        }
    }
}
