using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using Accord.MachineLearning.Geometry;
using Accord.Math;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GygaxCore.DataStructures;
using HelixToolkit.Wpf.SharpDX;
using MathNet.Numerics;
using SharpDX;
using Plane = SharpDX.Plane;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX.Core;
using ProtoBuf;

namespace GygaxVisu
{
    public class PlaneReconstructor
    {
        private readonly Quaternion _baseVector = new Quaternion(0, 0, 1, 0);

        private Image<Bgr, Byte> _image;
        
        private readonly List<CameraPosition> _cameraPositions;

        private readonly List<Patch> _patches;

        private ProjectionBase _projectionBase;

        private IfcMeshGeometryModel3D[] _model;

        public bool GenerateTextureFile;
        public bool GenerateMaskFile;

        public struct ProjectionBase
        {
            public Plane ProjectionPlane;
            public Vector3 BaseX
            {
                get { return Basis.Row1; }
                set { Basis.Row1 = value; }
            }

            public Vector3 BaseY
            {
                get { return Basis.Row2; }
                set { Basis.Row2 = value; }
            }

            public Vector3 BaseZ
            {
                get { return Basis.Row3; }
                set { Basis.Row3 = value; }
            }

            public Matrix3x3 Basis;

            public Matrix3x3 BasisInverse
            {
                get
                {
                    var m = Basis;
                    m.Invert();
                    return m;
                }
            }

            public double Distance;

        }

        public PlaneReconstructor(List<CameraPosition> cameraPositions, ProjectionBase projectionBase,
            List<Patch> patchList = null, IfcMeshGeometryModel3D[] model = null)
        {
            _cameraPositions = cameraPositions;
            _patches = patchList;

            _projectionBase = projectionBase;

            _model = model;

            _cameraPositions.Sort(Comparison);
            _cameraPositions.Reverse();
        }
        
        public void DrawGeometry(ref Image<Bgr, byte> image)
        {
            _image = image;
            var patches = new List<Vector2>();

            // Patch on plane
            if (_patches != null && _patches.Count > 0)
            {
                List<Ray> patchRays =
                    _patches.Select(patch => new Ray(patch.Position, _projectionBase.ProjectionPlane.Normal)).ToList();
                patches = Project2D(GetRayPlaneIntersection(patchRays));
            }

            // Camera Center on plane
            List<Ray> cameraCenterRays = _cameraPositions.Select(cameraPosition => new Ray(cameraPosition.CameraCenter.ToVector3(), _projectionBase.ProjectionPlane.Normal)).ToList();
            List<Vector2> cameraPositionsOnPlane = Project2D(GetRayPlaneIntersection(cameraCenterRays));

            // Camera Patch Intersection
            List<Ray> orientedCameraCenterRays = new List<Ray>();

            foreach (var cameraPosition in _cameraPositions)
            {
                //Changed Rotation
                var axis = CameraPosition.Rotate(cameraPosition.Orientation, _baseVector.Axis);

                Ray r = new Ray(
                    cameraPosition.CameraCenter.ToVector3(),
                    axis.ToVector3()
                    );

                orientedCameraCenterRays.Add(r);
            }

            List<Vector3D> cameraCenterPointPlaneIntersection3D = GetRayPlaneIntersection(orientedCameraCenterRays);
            List<Vector2> cameraCenterPointPlaneIntersection = Project2D(cameraCenterPointPlaneIntersection3D);

            // Get Corner Points of Image (depending on focal length)
            List<Vector2[]> cornerPoints = new List<Vector2[]>();
            foreach (var cameraPosition in _cameraPositions)
            {
                List<Ray> r = new List<Ray>
                {
                    new Ray(cameraPosition.CameraCenter.ToVector3(),
                        GetCornerPointToAxis(cameraPosition, cameraPosition.Orientation, MyProcessor.Direction.TopLeft).ToVector3()),
                    new Ray(cameraPosition.CameraCenter.ToVector3(),
                        GetCornerPointToAxis(cameraPosition, cameraPosition.Orientation, MyProcessor.Direction.TopRight).ToVector3()),
                    new Ray(cameraPosition.CameraCenter.ToVector3(),
                        GetCornerPointToAxis(cameraPosition, cameraPosition.Orientation, MyProcessor.Direction.BottomRight).ToVector3()),
                    new Ray(cameraPosition.CameraCenter.ToVector3(),
                        GetCornerPointToAxis(cameraPosition, cameraPosition.Orientation, MyProcessor.Direction.BottomLeft).ToVector3())
                };

                cornerPoints.Add(Project2D(GetRayPlaneIntersection(r)).ToArray());
            }

            // Fit it on screen
            CalculateImageFitting(patches, cameraPositionsOnPlane, cameraCenterPointPlaneIntersection);

            // From here on the drawing is realized
            foreach (var p in patches)
            {
                var pImg = FitOnImage(p);
                image[pImg.Y, pImg.X] = new Bgr(128, 128, 128);
            }

            foreach (var p in cameraPositionsOnPlane)
            {
                CvInvoke.Circle(image, FitOnImage(p), 3, new MCvScalar(255, 0, 0), 3);
            }

            foreach (var p in cameraCenterPointPlaneIntersection)
            {
                CvInvoke.Circle(image, FitOnImage(p), 3, new MCvScalar(0, 0, 255), 3);
            }

            for (int i = 0; i < cameraCenterPointPlaneIntersection.Count; i++)
            {
                var p = new System.Drawing.Point[4];

                for (int j = 0; j < 4; j++)
                {
                    p[j] = FitOnImage(cornerPoints[i][j]);
                }

                CvInvoke.Polylines(image, p, true, new MCvScalar(0, 255, 0));
                CvInvoke.Line(image, p[0], p[2], new MCvScalar(0, 255, 0));
                CvInvoke.Line(image, p[1], p[3], new MCvScalar(0, 255, 0));
            }

            DrawAxis();
        }

        //public static Vector3D Rotate(Quaternion q, Vector3D v)
        //{
        //    var conj = new Quaternion(q.X, q.Y, q.Z, q.W);
        //    conj.Conjugate();

        //    //Quaternion rotatedVector = conj * new Quaternion(v, 0) * q;
        //    Quaternion rotatedVector = q * new Quaternion(v, 0) * conj;

        //    return new Vector3D(rotatedVector.X, rotatedVector.Y, rotatedVector.Z);
        //}

        private int Comparison(CameraPosition position1, CameraPosition position2)
        {
            // distance 1
            Ray r = new Ray(position1.CameraCenter.ToVector3(), _projectionBase.ProjectionPlane.Normal);
            Vector3 v;
            _projectionBase.ProjectionPlane.Intersects(ref r, out v);
            var distance1 = (position1.CameraCenter.ToVector3() - v).Length();

            // distance 2
            Ray r2 = new Ray(position2.CameraCenter.ToVector3(), _projectionBase.ProjectionPlane.Normal);
            Vector3 v2;
            _projectionBase.ProjectionPlane.Intersects(ref r2, out v2);
            var distance2 = (position2.CameraCenter.ToVector3() - v2).Length();

            if (distance1 > distance2)
                return 1;

            return -1;
        }

        public double[,] SurfacePatchId;
        public double[,] SurfaceNoOfPixels;
        public double[,] SurfaceAngle;
        public double[,] SurfacePixelSize;

        public void ReconstructRayTracer(ref Image<Bgr, byte> surfaceReconstruction)
        {
            SurfacePatchId = new double[surfaceReconstruction.Width, surfaceReconstruction.Height];
            SurfaceNoOfPixels = new double[surfaceReconstruction.Width, surfaceReconstruction.Height];
            SurfaceAngle = new double[surfaceReconstruction.Width, surfaceReconstruction.Height];
            SurfacePixelSize = new double[surfaceReconstruction.Width, surfaceReconstruction.Height];

            for (int y = 0; y < surfaceReconstruction.Height; y++)
            {
                for (int x = 0; x < surfaceReconstruction.Width; x++)
                {
                    //RayTracerPixelInfo pixelInfo = GetClosestImageMatching(new System.Drawing.Point(x, y));

                    //if (pixelInfo.Intersections.Count > 0)
                    //{
                    //    surfaceReconstruction[y, x] = pixelInfo.Intersections[0].Color;
                    //}

                    //surfaceReconstruction[y, x] = new Bgr(0,255,255);

                    //pixelInfo.Intersections.Sort((e1, e2) => e1.PixelSize.CompareTo(e2.PixelSize));
                    ////pixelInfo.Intersections.Sort((e1, e2) => e1.Distance.CompareTo(e2.Distance));

                    //if (pixelInfo.Intersections.Count > 0)
                    //{
                    //    surfaceReconstruction[y, x] = pixelInfo.Intersections[0].Color;
                    //    SurfacePatchId[y, x] = pixelInfo.Intersections[0].CameraId;
                    //    SurfaceNoOfPixels[y, x] = pixelInfo.Intersections.Count;

                    //    SurfaceAngle[y, x] = (double)pixelInfo.Intersections[0].Angle;

                    //    SurfacePixelSize[y, x] = (double)pixelInfo.Intersections[0].PixelSize;
                    //}
                }
            }
        }

        //public void ReconstructRayTracer(ref Image<Bgr, byte> surfaceReconstruction, Triangle t, int textureWidth, int textureHeight)
        public void ReconstructRayTracer(ReconstructionContainer surfaceReconstruction, Triangle t, int textureWidth, int textureHeight)
        {
            var p1 = t.Corners[0].Coordinates3D;
            var p2 = t.Corners[1].Coordinates3D;
            var p3 = t.Corners[2].Coordinates3D;

            var u1 = t.Corners[0].CoordinatesTextureFloat;
            var u2 = t.Corners[1].CoordinatesTextureFloat;
            var u3 = t.Corners[2].CoordinatesTextureFloat;

            var vector1Image = u2 - u1;
            var vector13D = p2 - p1;

            var vector2Image = u3 - u1;
            var vector23D = p3 - p1;

            var minX = Math.Min(Math.Min(u1.X, u2.X), u3.X);
            var maxX = Math.Max(Math.Max(u1.X, u2.X), u3.X);

            var minY = Math.Min(Math.Min(u1.Y, u2.Y), u3.Y);
            var maxY = Math.Max(Math.Max(u1.Y, u2.Y), u3.Y);

            var yStart = (int) Math.Floor(minY);
            var yEnd = (int) Math.Ceiling(maxY);

            var xStart = (int) Math.Floor(minX);
            var xEnd = (int) Math.Ceiling(maxX);
            
            var list = surfaceReconstruction.Points.Where(q => !q.UpToDate).ToList();

            //Parallel.ForEach(list, item =>
            ////foreach (var item in list)
            //{
            //    var x = item.Index%textureWidth;
            //    var y = item.Index/textureWidth;

            //    //Console.WriteLine(x + " " + y);

            //    var m = Accord.Math.Matrix.Create(2, 3, 0.0);

            //    m[0, 0] = vector1Image.X;
            //    m[1, 0] = vector1Image.Y;

            //    m[0, 1] = vector2Image.X;
            //    m[1, 1] = vector2Image.Y;

            //    m[0, 2] = x - u1.X;
            //    m[1, 2] = y - u1.Y;

            //    var r = new ReducedRowEchelonForm(m);

            //    var a = (float) r.Result[0, 2];
            //    var b = (float) r.Result[1, 2];

            //    var result = a*vector1Image + b*vector2Image;
            //    var result3D = a*vector13D + b*vector23D;

            //    if (!PointInTriangle(u1 + result, u1, u2, u3))
            //    {
            //        return;
            //    }

            //    if (x < 0 || y < 0 || x >= surfaceReconstruction.Width || y >= surfaceReconstruction.Height)
            //        return;

            //    RayTracerPixelInfo pixelInfo = GetClosestImageMatching((p1 + result3D), t);

            //    if (pixelInfo.Intersections.Count == 0)
            //    {
            //        //surfaceReconstruction[y, x] = new Bgr(255, 0, 0);
            //        return;
            //    }

            //    //surfaceReconstruction[y, x] = GetBestPixel(pixelInfo.Intersections).Color;

            //    //surfaceReconstruction.Points[y, x] = GetBestPixel(pixelInfo.Intersections);
            //    //surfaceReconstruction.Points[y * surfaceReconstruction.Width + x] = GetBestPixel(pixelInfo.Intersections);

            //    var bestPixel = GetBestPixel(pixelInfo.Intersections);
            //    bestPixel.UpToDate = true;
            //    surfaceReconstruction.Points[y*textureWidth + x] = bestPixel;
            //});


            //}

            Parallel.For(yStart, yEnd, y =>
            //for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                //Parallel.For(xStart, xEnd, x =>
                {
                    var m = Accord.Math.Matrix.Create(2, 3, 0.0);

                    m[0, 0] = vector1Image.X;
                    m[1, 0] = vector1Image.Y;

                    m[0, 1] = vector2Image.X;
                    m[1, 1] = vector2Image.Y;

                    m[0, 2] = x - u1.X;
                    m[1, 2] = y - u1.Y;

                    var r = new ReducedRowEchelonForm(m);

                    var a = (float) r.Result[0, 2];
                    var b = (float) r.Result[1, 2];

                    var result = a*vector1Image + b*vector2Image;
                    var result3D = a*vector13D + b*vector23D;

                    if (!PointInTriangle(u1 + result, u1, u2, u3))
                    {
                        continue;
                    }

                    if (x < 0 || y < 0 || x >= surfaceReconstruction.Width || y >= surfaceReconstruction.Height)
                        continue;

                    RayTracerPixelInfo pixelInfo = GetClosestImageMatching((p1 + result3D), t);

                    if (pixelInfo.Intersections.Count == 0)
                    {
                        //surfaceReconstruction[y, x] = new Bgr(255, 0, 0);
                        continue;
                    }

                    //surfaceReconstruction[y, x] = GetBestPixel(pixelInfo.Intersections).Color;

                    //surfaceReconstruction.Points[y, x] = GetBestPixel(pixelInfo.Intersections);
                    //surfaceReconstruction.Points[y * surfaceReconstruction.Width + x] = GetBestPixel(pixelInfo.Intersections);

                    var bestPixel = GetBestPixel(pixelInfo.Intersections);
                    surfaceReconstruction.Points[y*textureWidth + x] = bestPixel;
                }
            });
            //}
        }

        private void ExtractPixel(Vector2 vector1Image, Vector2 vector2Image, int x, int y, Vector2 u1, Vector2 u2, Vector2 u3, Vector3D vector13D, Vector3D vector23D)
        {

        }


        private RayTracerPixelInfo.Intersection GetBestPixel(List<RayTracerPixelInfo.Intersection> candidates)
        {
            //First, angle < 45 deg and smallest pixel
            var ordered = candidates.Where(x => x.Angle > 0.25*Math.PI);

            if (ordered.Any())
                return ordered.OrderBy(x => x.PixelSize).First();

            //Get steepest angle
            return candidates.OrderByDescending(x => x.Angle).First();


            ////Second, angle < 60 deg and smallest pixel
            //ordered = candidates.Where(x => x.Angle > 0.334 * Math.PI);

            //if (ordered.Any())
            //    return ordered.OrderBy(x => x.PixelSize).First();

            ////Third, angle < 60 deg and smallest pixel
            //ordered = candidates.Where(x => x.Angle > 0.4 * Math.PI);

            //if (ordered.Any())
            //    return ordered.OrderBy(x => x.PixelSize).First();

            ////Finally, pick the smallest pixel
            //return candidates.OrderByDescending(x => x.Angle).First();
        }


        private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            var p0x = a.X;
            var p0y = a.Y;
            var p1x = b.X;
            var p1y = b.Y;
            var p2x = c.X;
            var p2y = c.Y;
            var px = p.X;
            var py = p.Y;

            var area = 1f / 2 * (-p1y * p2x + p0y * (-p1x + p2x) + p0x * (p1y - p2y) + p1x * p2y);

            var s = 1f / (2 * area) * (p0y * p2x - p0x * p2y + (p2y - p0y) * px + (p0x - p2x) * py);
            var t = 1f / (2 * area) * (p0x * p1y - p0y * p1x + (p0y - p1y) * px + (p1x - p0x) * py);

            return (s >= 0) && (t >= 0) && (1 - s - t >= 0);
        }

        public struct RayTracerPixelInfo
        {
            [ProtoContract]
            public struct Intersection
            {
                [ProtoMember(1)]
                public int ImageX;
                [ProtoMember(2)]
                public int ImageY;
                
                public CameraPosition CameraPosition;

                
                public Bgr Color;

                [ProtoMember(3)]
                public string Filename;

                [ProtoMember(4)]
                public double Distance;
                [ProtoMember(5)]
                public double PixelSize;
                [ProtoMember(6)]
                public double Angle;
                [ProtoMember(7)]
                public int CameraId;
                [ProtoMember(8)]
                public bool Intersects;

                public bool UpToDate;
                public int Index;
            }

            public Bgr Color;
            public List<RayTracerPixelInfo.Intersection> Intersections;

            public RayTracerPixelInfo(List<Intersection> intersections)
            {
                Intersections = intersections;
                Color = new Bgr(128, 0, 0);
            }
        }

        private RayTracerPixelInfo GetClosestImageMatching(SharpDX.Point p, Triangle t)
        {
            // Get 3D coordinates to p -> plane point
            var x = ((p.X - _image.Width / 2.0) / _scalingFactor) + _centerX;
            var y = ((p.Y - _image.Height / 2.0) / _scalingFactor) + _centerY;

            var pp = new Vector3D(x, y, _projectionBase.Distance);

            var planePoint = MultiplyMatrixVector(_projectionBase.BasisInverse, pp);

            return GetClosestImageMatching(planePoint, t);
        }

        public Dictionary<Triangle, List<CameraPosition>> Dict;
        public Dictionary<CameraPosition, List<Triangle>> TrianglesToConsider;

        private RayTracerPixelInfo GetClosestImageMatching(Vector3D planePoint, Triangle t)
        {
            var returnValue = new RayTracerPixelInfo(new List<RayTracerPixelInfo.Intersection>());
            
            //foreach (var cameraPosition in t.TrianglesToConsider.Keys)
            //foreach (var cameraPosition in _cameraPositions)
            foreach(var cameraPosition in Dict[t])
            {
                RayTracerPixelInfo.Intersection intersection;

                switch(cameraPosition.Type)
                {
                    case CameraPosition.CameraType.Planar:
                        intersection = GetClosestImageMatchingPlanar(planePoint, cameraPosition, t);
                        break;

                    case CameraPosition.CameraType.Spherical:
                        intersection = GetClosestImageMatchingSpherical(planePoint, cameraPosition, t);
                        break;

                    default:
                        continue;
                }

                if (intersection.Intersects)
                {
                    returnValue.Intersections.Add(intersection);
                }
            }

            //returnValue.Intersections.Sort((x, y) => x.Distance.CompareTo(y.Distance));
            //returnValue.Intersections.Sort((x, y) => x.PixelSize.CompareTo(y.PixelSize));
            //returnValue.Intersections.Sort((x, y) => y.Angle.CompareTo(x.Angle));


            
            return returnValue;
        }
        
        private RayTracerPixelInfo.Intersection GetClosestImageMatchingSpherical(Vector3D planePoint, CameraPosition cameraPosition, Triangle t)
        {
            var d = cameraPosition.CameraCenter - planePoint;
            d.Normalize();

            //Rotate changed
            d = CameraPosition.Rotate(cameraPosition.Orientation, d);

            var u = 0.5 + Math.Atan2(d.Z, d.X) / (2 * Math.PI);
            var v = 0.5 + Math.Asin(d.Y) / Math.PI;

            var widthFactor = (cameraPosition.OpeningAngleHorizontalTo - cameraPosition.OpeningAngleHorizontalFrom) / (2 * Math.PI);
            var heightFactor = (cameraPosition.OpeningAngleVerticalFrom - cameraPosition.OpeningAngleVerticalTo) / Math.PI;

            var startHorizontal = cameraPosition.OpeningAngleHorizontalFrom / (2 * Math.PI);
            var startVertical = cameraPosition.OpeningAngleVerticalFrom / Math.PI - 0.5;

            u = (u - startHorizontal) / widthFactor + startHorizontal;
            v = (v - startVertical) / heightFactor + startVertical;

            var uImg = (int)Math.Round(u * cameraPosition.Width);
            var vImg = (int)Math.Round(v * cameraPosition.Height);

            if (uImg >= cameraPosition.Width || vImg >= cameraPosition.Height)
            {
                return new RayTracerPixelInfo.Intersection()
                {
                    Intersects = false
                };
            }

            if (HitsAnotherTriangleFirst(planePoint.ToVector3(), cameraPosition, t))
            {
                return new RayTracerPixelInfo.Intersection()
                {
                    Intersects = false
                };
            }

            var c = cameraPosition.Image[vImg, uImg];
            
            return new RayTracerPixelInfo.Intersection()
            {
                Color = c,
                Intersects = true,
                Distance = (planePoint - cameraPosition.CameraCenter).Length,
                //Angle = Math.Acos(Vector3.Dot(new SharpDX.Plane(t.ActiveConfiguration.Corners[0].Coordinates3D, t.ActiveConfiguration.Corners[1].Coordinates3D, t.ActiveConfiguration.Corners[2].Coordinates3D).Normal, (planePoint - cameraPosition.CameraCenter).ToVector3())) / (new SharpDX.Plane(t.ActiveConfiguration.Corners[0].Coordinates3D, t.ActiveConfiguration.Corners[1].Coordinates3D, t.ActiveConfiguration.Corners[2].Coordinates3D).Normal.Length() * (planePoint - cameraPosition.CameraCenter).Length),
                //CameraId = cameraPosition.Id,
                //Angle = Math.Acos(Vector3.Dot(imagePlane.Normal, rayPlanePoint.Direction) / (imagePlane.Normal.Length() * rayPlanePoint.Direction.Length())),
                //PixelSize = (planePoint - cameraPosition.CameraCenter).Length() / cameraPosition.FocalLength
            };
        }

        public List<Triangle> Triangles = new List<Triangle>();

        private bool HitsAnotherTriangleFirst(Vector3 planePoint, CameraPosition cameraPosition, Triangle t)
        {
            var ray = new Ray(cameraPosition.CameraCenter.ToVector3(), (planePoint - cameraPosition.CameraCenter.ToVector3()).Normalized());
            var planeDistance = (planePoint - cameraPosition.CameraCenter.ToVector3()).Length();

            float intersectionDistance;

            //Stopwatch.Start();

            //foreach (var triangle in t.TrianglesToConsider[cameraPosition])
            //foreach (var triangle in TrianglesToConsider[t])
            foreach (var triangle in TrianglesToConsider[cameraPosition])
            {
                if (!Collision.RayIntersectsTriangle(
                    ref ray,
                    ref triangle.Corners[0].Coordinates3DFloat,
                    ref triangle.Corners[1].Coordinates3DFloat,
                    ref triangle.Corners[2].Coordinates3DFloat,
                    out intersectionDistance)
                    )
                    continue;

                if (intersectionDistance < planeDistance-0.01)
                    return true;
            }

            //Stopwatch.Stop();

            return false;
        }

        private Plane[] imagePlanes;
        private bool[] rayCentersIntersect;
        private Vector3[] intersectionCenters;
        private Vector3D[] correctedCenters;


        public void Init()
        {
            imagePlanes = new Plane[_cameraPositions.Count];
            rayCentersIntersect = new bool[_cameraPositions.Count];
            intersectionCenters = new Vector3[_cameraPositions.Count];
            correctedCenters = new Vector3D[_cameraPositions.Count];

            foreach (var cameraPosition in _cameraPositions)
            {
                imagePlanes[cameraPosition.Id] =
                    new Plane(
                        (cameraPosition.CameraCenter + cameraPosition.Normal * cameraPosition.FocalLength).ToVector3(),
                        cameraPosition.Normal.ToVector3());

                var rayCenter = new Ray(cameraPosition.CameraCenter.ToVector3(), cameraPosition.Normal.ToVector3());

                Vector3 intersectionCenter;

                rayCentersIntersect[cameraPosition.Id] =
                    imagePlanes[cameraPosition.Id].Intersects(ref rayCenter, out intersectionCenter);

                intersectionCenters[cameraPosition.Id] = intersectionCenter;
                correctedCenters[cameraPosition.Id] = MultiplyMatrixVector(cameraPosition.Basis, new Vector3D(intersectionCenter.X, intersectionCenter.Y, intersectionCenter.Z));
            }
        }

        private RayTracerPixelInfo.Intersection GetClosestImageMatchingPlanar(Vector3D planePoint, CameraPosition cameraPosition, Triangle t)
        {
            if (HitsAnotherTriangleFirst(planePoint.ToVector3(), cameraPosition, t))
            {
                return new RayTracerPixelInfo.Intersection()
                {
                    Intersects = false
                };
            }

            if (!rayCentersIntersect[cameraPosition.Id])
                return NoIntersection;

            var rayPlanePoint = new Ray(cameraPosition.CameraCenter.ToVector3(), (planePoint - cameraPosition.CameraCenter).ToVector3().Normalized());

            Vector3 intersectionPlanePoint;
            
            if (!imagePlanes[cameraPosition.Id].Intersects(ref rayPlanePoint, out intersectionPlanePoint))
                return NoIntersection;
            
            var correctedPlanePoint = MultiplyMatrixVector(cameraPosition.Basis, new Vector3D(intersectionPlanePoint.X, intersectionPlanePoint.Y, intersectionPlanePoint.Z));
            
            var ximg = correctedPlanePoint.X - correctedCenters[cameraPosition.Id].X;
            var yimg = correctedPlanePoint.Y - correctedCenters[cameraPosition.Id].Y;

            ximg = ximg / cameraPosition.ImageDiagonal;
            yimg = yimg / cameraPosition.ImageDiagonal;

            //var pCorrected = Distort(new []{ximg, yimg}, cameraPosition.RadialDistortion);

            //ximg = (pCorrected[0] * cameraPosition.ImageDiagonal + cameraPosition.Width / 2.0);
            //yimg = (pCorrected[1] * cameraPosition.ImageDiagonal + cameraPosition.Height / 2.0);

            ximg = (ximg * cameraPosition.ImageDiagonal + cameraPosition.Width / 2.0);
            yimg = (yimg * cameraPosition.ImageDiagonal + cameraPosition.Height / 2.0);

            ximg = Math.Round(ximg);
            yimg = Math.Round(yimg);



            if (double.IsNaN(ximg) || double.IsNaN(yimg) || ximg < 0 || yimg < 0 || ximg >= cameraPosition.Width || yimg >= cameraPosition.Height)
                return NoIntersection;
            
            return new RayTracerPixelInfo.Intersection()
            {
                ImageX = Convert.ToInt32(ximg),
                ImageY = Convert.ToInt32(yimg),
                CameraPosition = cameraPosition,
                Filename = cameraPosition.File,
                Distance = (planePoint - cameraPosition.CameraCenter).Length,
                CameraId = cameraPosition.Id,
                Angle = Math.Asin(Vector3.Dot(rayPlanePoint.Direction, t.Normal)),
                PixelSize = (planePoint - cameraPosition.CameraCenter).Length / cameraPosition.FocalLength,
                Intersects = true
            };
        }

        RayTracerPixelInfo.Intersection NoIntersection = new RayTracerPixelInfo.Intersection { Intersects = false };

        //private RayTracerPixelInfo.Intersection GetClosestImageMatchingPlanar(Vector3D planePoint, CameraPosition cameraPosition, Triangle t)
        //{
        //    //if (HitsAnotherTriangleFirst(planePoint.ToVector3(), cameraPosition, t))
        //    //{
        //    //    return new RayTracerPixelInfo.Intersection()
        //    //    {
        //    //        Intersects = false
        //    //    };
        //    //}

        //    //var imagePlane = new Plane((cameraPosition.CameraCenter + cameraPosition.Normal * cameraPosition.FocalLength).ToVector3(), cameraPosition.Normal.ToVector3());

        //    if (!rayCentersIntersect[cameraPosition.Id])
        //        return NoIntersection;

        //    var rayPlanePoint = new Ray(cameraPosition.CameraCenter.ToVector3(), (planePoint - cameraPosition.CameraCenter).ToVector3());
            
        //    Vector3 intersectionPlanePoint;
        //    if (!imagePlanes[cameraPosition.Id].Intersects(ref rayPlanePoint, out intersectionPlanePoint))
        //        return NoIntersection;
            
        //    var correctedPlanePoint = MultiplyMatrixVector(cameraPosition.Basis, new Vector3D(intersectionPlanePoint.X, intersectionPlanePoint.Y, intersectionPlanePoint.Z));

        //    var ximg = correctedPlanePoint.X - intersectionCenters[cameraPosition.Id].X;
        //    var yimg = correctedPlanePoint.Y - intersectionCenters[cameraPosition.Id].Y;
            
        //    ximg = ximg / cameraPosition.ImageDiagonal;
        //    yimg = yimg / cameraPosition.ImageDiagonal;

        //    //var pCorrected = Distort(new []{ximg, yimg}, cameraPosition.RadialDistortion);

        //    //ximg = (pCorrected[0] * cameraPosition.ImageDiagonal + cameraPosition.Width / 2.0);
        //    //yimg = (pCorrected[1] * cameraPosition.ImageDiagonal + cameraPosition.Height / 2.0);

        //    ximg = (ximg * cameraPosition.ImageDiagonal + cameraPosition.Width / 2.0);
        //    yimg = (yimg * cameraPosition.ImageDiagonal + cameraPosition.Height / 2.0);

        //    ximg = Math.Round(ximg);
        //    yimg = Math.Round(yimg);



        //    if (double.IsNaN(ximg) || double.IsNaN(yimg) || ximg < 0 || yimg < 0 || ximg >= cameraPosition.Width || yimg >= cameraPosition.Height)
        //        return NoIntersection;



        //    //if (cameraPosition.Image == null)
        //    //{
        //    //    try
        //    //    {
        //    //        cameraPosition.Image = new Image<Bgr, Byte>(cameraPosition.File);
        //    //    }
        //    //    catch (Exception)
        //    //    {
        //    //        Debug.WriteLine("Source image for reconstruction is null. Load it first.");
        //    //        return new RayTracerPixelInfo.Intersection { Intersects = false };
        //    //    }
        //    //}

        //    //if(GenerateMaskFile)
        //    //    SetMask(cameraPosition, Convert.ToInt32(yimg), Convert.ToInt32(ximg));

        //    var rayDirection = rayPlanePoint.Direction.Normalized();
            
            
        //    return new RayTracerPixelInfo.Intersection()
        //    {
        //        ImageX = Convert.ToInt32(ximg),
        //        ImageY = Convert.ToInt32(yimg),
        //        CameraPosition = cameraPosition,
        //        //Color = cameraPosition.Image[Convert.ToInt32(yimg), Convert.ToInt32(ximg)],
        //        Filename = cameraPosition.File,
        //        Distance = (planePoint - cameraPosition.CameraCenter).Length,
        //        CameraId = cameraPosition.Id,
        //        Angle = Math.Asin(Vector3.Dot(rayDirection, t.Normal)),
        //        PixelSize = (planePoint - cameraPosition.CameraCenter).Length / cameraPosition.FocalLength,
        //        Intersects = true
        //    };
        //}

        readonly Dictionary<string, Image<Gray, byte>> maskImages = new Dictionary<string, Image<Gray, byte>>(); 

        private void SetMask(CameraPosition cameraPosition, int y, int x)
        {
            var file = Path.GetDirectoryName(cameraPosition.File) + @"\Masks\" + Path.GetFileName(cameraPosition.File);
            //file = @"Z:\06. Data\Bridges\Philipp\Bridge 1\Processed\101COLUM\" + file?.Split('.')[0] + "_mask.jpg";

            if (!maskImages.ContainsKey(file))
            {
                Image<Gray, byte> img = new Image<Gray, byte>(cameraPosition.Width, cameraPosition.Height);

                maskImages.Add(file, img);
            }

            maskImages[file][y, x] = new Gray(255);
        }
        
        private static Matrix3x3 GetBasis(Vector3 normal, Vector3 point1, Vector3 point2)
        {
            var returnValue = new Matrix3x3();

            // Calculate corresponding basis
            var z = normal;
            z.Normalize();
            returnValue.Row3 = z;

            var u = point2 - point1;
            returnValue.Row1 = u - Vector3.Cross(Vector3.Cross(u, z), z);
            returnValue.Row1.Normalize();

            returnValue.Row2 = Vector3.Cross(z, returnValue.Row1);

            return returnValue;
        }

        private void ReconstructSurfaceImage(List<Vector2[]> cornerPoints)
        {
            throw new NotImplementedException();

            //Image<Bgr, Byte> bufferImage = new Image<Bgr, Byte>(new Size(_imageSideLength, _imageSideLength));

            //for (int i = 0; i < _cameraPositions.Count; i++)
            //{
            //    var cameraPosition = _cameraPositions[i];
            //    var corners = cornerPoints[i];

            //    var cornersFImg = new PointF[4];
            //    cornersFImg[3] = new PointF(0, 0);
            //    cornersFImg[0] = new PointF(cameraPosition.Width - 1, 0);
            //    cornersFImg[1] = new PointF(cameraPosition.Width - 1, cameraPosition.Height - 1);
            //    cornersFImg[2] = new PointF(0, cameraPosition.Height - 1);

            //    var cornersF = new PointF[4];
            //    cornersF[0] = FitOnImageF(corners[0]);
            //    cornersF[1] = FitOnImageF(corners[1]);
            //    cornersF[2] = FitOnImageF(corners[2]);
            //    cornersF[3] = FitOnImageF(corners[3]);

            //    var cornersP = new System.Drawing.Point[4];
            //    cornersP[0] = FitOnImage(corners[0]);
            //    cornersP[1] = FitOnImage(corners[1]);
            //    cornersP[2] = FitOnImage(corners[2]);
            //    cornersP[3] = FitOnImage(corners[3]);

            //    Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(cameraPosition.File.LocalPath);

            //    Mat points = CvInvoke.GetPerspectiveTransform(cornersFImg, cornersF);
            //    CvInvoke.WarpPerspective(img1, bufferImage, points, bufferImage.Size);

            //    Image<Gray, Byte> mask = new Image<Gray, Byte>(bufferImage.Size);
            //    CvInvoke.FillConvexPoly(mask, new VectorOfPoint(cornersP), new MCvScalar(255));

            //    bufferImage.Copy(_surfaceReconstruction, mask);
            //}

            //_surfaceReconstruction.Save(@"C:\Users\Philipp\Desktop\test2.bmp");
        }

        private System.Drawing.Point FitOnImage(Vector2 p)
        {
            var y = Convert.ToInt32((p.Y - _centerY) * _scalingFactor + _image.Height / 2.0);
            var x = Convert.ToInt32((p.X - _centerX) * _scalingFactor + _image.Width / 2.0);

            return new System.Drawing.Point(x, y);
        }

        private PointF FitOnImageF(Vector2 p)
        {
            var v = FitOnImage(p);

            return new PointF(v.X, v.Y);
        }

        // Scaling parameters
        private double _scalingFactor;
        private double _centerX;
        private double _centerY;

        private void CalculateImageFitting(List<Vector2> l1, List<Vector2> l2, List<Vector2> l3)
        {
            var minX = Math.Min(Math.Min(l1.Min(e => e.X), l2.Min(e => e.X)), l3.Min(e => e.X));
            var maxX = Math.Max(Math.Max(l1.Max(e => e.X), l2.Max(e => e.X)), l3.Max(e => e.X));
            var widthX = maxX - minX;
            _centerX = widthX / 2.0 + minX;

            var minY = Math.Min(Math.Min(l1.Min(e => e.Y), l2.Min(e => e.Y)), l3.Min(e => e.Y));
            var maxY = Math.Max(Math.Max(l1.Max(e => e.Y), l2.Max(e => e.Y)), l3.Max(e => e.Y));
            var widthY = maxY - minY;
            _centerY = widthY / 2.0 + minY;

            const double margin = 0.9;
            var scalingFactorX = (margin * _image.Height) / widthY;
            var scalingFactorY = (margin * _image.Width) / widthX;
            _scalingFactor = Math.Min(scalingFactorX, scalingFactorY);
        }

        public static Vector3D GetCornerPointToAxis(CameraPosition cameraPosition, Quaternion axis, MyProcessor.Direction direction)
        {
            // Orientation in Viewing direction
            var a1 = cameraPosition.Width / 2;
            var a2 = cameraPosition.Height / 2;
            var a3 = cameraPosition.FocalLength;

            var angleY = Math.Atan(a1 / a3);
            var angleX = Math.Atan(a2 / a3);

            // Set sign for corner
            switch (direction)
            {
                default:
                case MyProcessor.Direction.TopLeft:
                    break;
                case MyProcessor.Direction.TopRight:
                    angleX = -angleX;
                    break;
                case MyProcessor.Direction.BottomLeft:
                    angleY = -angleY;
                    break;
                case MyProcessor.Direction.BottomRight:
                    angleX = -angleX;
                    angleY = -angleY;
                    break;
            }

            // Rotation matrices
            var xm = new Matrix3D
            {
                M11 = 1,
                M22 = Math.Cos(angleX),
                M32 = Math.Sin(angleX),
                M23 = -Math.Sin(angleX),
                M33 = Math.Cos(angleX)
            };

            var ym = new Matrix3D
            {
                M11 = Math.Cos(angleY),
                M31 = -Math.Sin(angleY),
                M22 = 1,
                M13 = Math.Sin(angleY),
                M33 = Math.Cos(angleY)
            };

            // Rotate the point by multiplying with quaternion and conjugate of quaternion
            //var rot = Quaternion.RotationMatrix(xm) * Quaternion.RotationMatrix(ym) * axis;
            var rot = CameraPosition.TransformToQuaternion(xm)*CameraPosition.TransformToQuaternion(ym)*axis;

            //Rotate changed
            return CameraPosition.Rotate(rot, new Vector3D(0,0,1));
        }

        public static ProjectionBase ExtractProjectionPlane(List<Patch> patches, List<CameraPosition> cameraPositions)
        {
            var localProjectionBase = new ProjectionBase();

            // Get Projection Plane
            Point3[] list = new Point3[patches.Count];

            for (int i = 0; i < patches.Count; i++)
            {
                list[i] = new Point3((float)patches[i].Position.X, (float)patches[i].Position.Y, (float)patches[i].Position.Z);
            }

            RansacPlane rp = new RansacPlane(0.95, 0.95);
            var plane = rp.Estimate(list);
            var normal = new Vector3(plane.Normal.ToArray());

            localProjectionBase.ProjectionPlane = new Plane(-normal, -plane.Offset);

            // Get average orientation of camera around z-axis
            var a = new List<double>();

            foreach (var c in cameraPositions)
            {
                var o = c.Orientation;
                a.Add(Math.Atan2(2 * o.Y * o.Y + 2 * o.Z * o.W, 1 - 2 * Math.Pow(o.Y, 2) - 2 * Math.Pow(o.Z, 2)));
            }

            var alpha = -a.Average();

            // rotate unit vector
            var v = new Vector3((float)Math.Cos(alpha), (float)Math.Sin(alpha), 0);

            // check for intersection with projection plane
            // carefull, could end up in error if parallel to either x or z
            var r1 = new Ray(Vector3.Zero, v);
            Vector3 point1;
            if (!localProjectionBase.ProjectionPlane.Intersects(ref r1, out point1))
            {
                r1 = new Ray(Vector3.Zero, -v);
                localProjectionBase.ProjectionPlane.Intersects(ref r1, out point1);
            }

            var r2 = new Ray(Vector3.Zero, Vector3.UnitZ);
            Vector3 point2;
            if (!localProjectionBase.ProjectionPlane.Intersects(ref r2, out point2))
            {
                r1 = new Ray(Vector3.Zero, -Vector3.UnitZ);
                localProjectionBase.ProjectionPlane.Intersects(ref r2, out point2);
            }

            var angleO = new AngleSingle(290, AngleType.Degree);
            var angle = angleO.Radians;
            var rotationMatrix = new Matrix3x3()
            {
                M11 = (float)Math.Cos(angle),
                M12 = (float)-Math.Sin(angle),
                M21 = (float)Math.Sin(angle),
                M22 = (float)Math.Cos(angle),
                M33 = 1
            };

            // Calculate corresponding basis
            localProjectionBase.Basis = GetBasis(localProjectionBase.ProjectionPlane.Normal,
                MultiplyMatrixVector(rotationMatrix,
                    new Vector3D(-localProjectionBase.ProjectionPlane.D / localProjectionBase.ProjectionPlane.Normal.X, 0,
                        0)).ToVector3(),
                MultiplyMatrixVector(rotationMatrix,
                    new Vector3D(0, -localProjectionBase.ProjectionPlane.D / localProjectionBase.ProjectionPlane.Normal.Y,
                        0)).ToVector3()
                );

            localProjectionBase.Distance = -localProjectionBase.ProjectionPlane.D;

            return localProjectionBase;
        }

        private List<Vector3D> GetRayPlaneIntersection(List<Ray> rays)
        {
            var intersectionPoints = new List<Vector3D>();

            foreach (var r in rays)
            {
                intersectionPoints.Add(GetRayPlaneIntersection(r));
            }

            return intersectionPoints;
        }

        private Vector3D GetRayPlaneIntersection(Ray ray)
        {
            Vector3 res;

            var intersection = _projectionBase.ProjectionPlane.Intersects(ref ray, out res);

            // check if it is behind plane
            if (!intersection)
            {
                var rReverse = new Ray(ray.Position, -ray.Direction);

                intersection = _projectionBase.ProjectionPlane.Intersects(ref rReverse, out res);
            }

            return new Vector3D(res.X, res.Y, res.Z);
        }

        private List<Vector2> Project2D(List<Vector3D> vectorList)
        {
            var returnList = new List<Vector2>();

            foreach (var vector in vectorList)
            {
                var v = MultiplyMatrixVector(_projectionBase.Basis, vector);
                //returnList.Add(new Vector2(Vector3.Dot(vector, projectionBase.BaseX), Vector3.Dot(vector, projectionBase.BaseY)));
                returnList.Add(new Vector2((float)v.X, (float)v.Y));
            }

            return returnList;
        }

        public static Vector3D MultiplyMatrixVector(Matrix3x3 m, Vector3D v)
        {
            return new Vector3D(m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z, m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z, m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z);
        }

        private void DrawAxis()
        {
            const int lengthOfAxis = 20;
            MCvScalar color = new MCvScalar(0, 255, 255);

            Matrix3x3 baseMatrix = new Matrix3x3
            {
                Column1 = _projectionBase.BaseX,
                Column2 = _projectionBase.BaseY
            };

            Matrix3x3 axisEndpoint = new Matrix3x3();

            Matrix3x3 mBuffer;

            // x-axis
            axisEndpoint.Row1 = new Vector3(-lengthOfAxis / 2f, 0, 0);
            axisEndpoint.Row2 = new Vector3(lengthOfAxis / 2f, 0, 0);

            mBuffer = axisEndpoint * baseMatrix;

            var xn = FitOnImage(new Vector2(mBuffer.M11, mBuffer.M12));
            var xp = FitOnImage(new Vector2(mBuffer.M21, mBuffer.M22));

            CvInvoke.ArrowedLine(_image, xn, xp, new MCvScalar(0, 0, 255));

            // y-axis
            axisEndpoint.Row1 = new Vector3(0, -lengthOfAxis / 2f, 0);
            axisEndpoint.Row2 = new Vector3(0, lengthOfAxis / 2f, 0);

            mBuffer = axisEndpoint * baseMatrix;

            var yn = FitOnImage(new Vector2(mBuffer.M11, mBuffer.M12));
            var yp = FitOnImage(new Vector2(mBuffer.M21, mBuffer.M22));

            CvInvoke.ArrowedLine(_image, yn, yp, new MCvScalar(0, 255, 0));

            // z-axis
            axisEndpoint.Row1 = new Vector3(0, 0, -lengthOfAxis / 2f);
            axisEndpoint.Row2 = new Vector3(0, 0, lengthOfAxis / 2f);

            mBuffer = axisEndpoint * baseMatrix;

            var zn = FitOnImage(new Vector2(mBuffer.M11, mBuffer.M12));
            var zp = FitOnImage(new Vector2(mBuffer.M21, mBuffer.M22));

            CvInvoke.ArrowedLine(_image, zn, zp, new MCvScalar(255, 0, 0));
        }

        public static double[] Distort(double[] pt, double radialDistortion)
        {
            if (radialDistortion == 0.0)
                return pt;

            var x = pt[0];
            var y = pt[1];

            if (y == 0)
                y = 1e-12f;

            double t2 = y * y;
            double t3 = t2 * t2 * t2;
            double t4 = x * x;
            double t7 = radialDistortion * (t2 + t4);

            if (radialDistortion > 0)
            {
                double t8 = 1.0 / t7;
                double t10 = t3 / (t7 * t7);
                double t14 = Math.Sqrt(t10 * (0.25 + t8 / 27.0));
                double t15 = t2 * t8 * y * 0.5;
                double t17 = Math.Pow(t14 + t15, 1.0 / 3.0);
                double t18 = t17 - t2 * t8 / (t17 * 3.0);
                return new [] { (t18*x/y), t18 };
            }
            else
            {
                double t9 = t3 / (t7 * t7 * 4.0);
                double t11 = t3 / (t7 * t7 * t7 * 27.0);
                Complex t12 = t9 + t11;
                Complex t13 = t12.SquareRoot();
                double t14 = t2 / t7;
                double t15 = t14 * y * 0.5;
                Complex t16 = t13 + t15;
                Complex t17 = t16.Power(1.0 / 3.0);
                Complex t18 = (t17 + t14 / (t17 * 3.0)) * new Complex(0.0, Math.Sqrt(3.0));
                Complex t19 = -0.5 * (t17 + t18) + t14 / (t17 * 6.0);
                return new[] { (t19.Real * x / y), t19.Real };
            }
        }

        public void SaveMasks()
        {
            foreach (var maskImage in maskImages)
            {
                maskImage.Value._Dilate(50);
                maskImage.Value._Erode(50);
                maskImage.Value.Save(maskImage.Key);
            }
        }
    }

    [Serializable]
    [ProtoContract]
    public class ReconstructionContainer
    {
        public const int chunkSize = 1000000;

        [ProtoMember(1)]
        public int Width;
        [ProtoMember(2)]
        public int Height;

        [ProtoMember(3)]
        public readonly PlaneReconstructor.RayTracerPixelInfo.Intersection[] Points;

        [ProtoMember(4)]
        public int IndexStart;
        [ProtoMember(5)]
        public int IndexEnd;
        [ProtoMember(6)]
        public int TotalNumberOfElements;


        public ReconstructionContainer()
        {
        }

        public ReconstructionContainer(int width, int height) : this(width, height, width*height)
        {
        }

        public ReconstructionContainer(int width, int height, int arraySize)
        {
            Width = width;
            Height = height;

            IndexStart = 0;
            IndexEnd = width * height;

            Points = new PlaneReconstructor.RayTracerPixelInfo.Intersection[arraySize];

            TotalNumberOfElements = width * height;
        }

        public static ReconstructionContainer Read(string filename)
        {
            ReconstructionContainer init;

            try
            {
                using (var file = File.OpenRead(filename))
                {
                    init = Serializer.Deserialize<ReconstructionContainer>(file);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            var r = new ReconstructionContainer(init.Width, init.Height, init.TotalNumberOfElements);
            Array.Copy(init.Points, 0, r.Points, init.IndexStart, init.IndexEnd - init.IndexStart);

            var numberOfChunks = (int) Math.Ceiling((init.TotalNumberOfElements - 1.0)/(init.IndexEnd - init.IndexStart));

            for (int i = 1; i < numberOfChunks; i++)
            {
                try
                {
                    using (var file = File.OpenRead(filename.Replace(".0.bin", "." + i + ".bin")))
                    {
                        var l = Serializer.Deserialize<ReconstructionContainer>(file);
                        Array.Copy(l.Points,0,r.Points, l.IndexStart, l.IndexEnd - l.IndexStart);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            for (int i = 0; i < r.Points.Length; i++)
            {
                r.Points[i].UpToDate = true;
                r.Points[i].Index = i;
            }

            return r;
        }

        public void Save(string filename)
        {
            //FileStream stream = File.Create(filename+".bin");
            //var formatter = new BinaryFormatter();
            //formatter.Serialize(stream, this);
            //stream.Close();

            int numberOfChunks = (int)Math.Ceiling(Points.Length * 1.0/chunkSize);


            Parallel.For(0, numberOfChunks, j =>
            {
                var i = j*chunkSize;

                var indexStart = i;
                var indexEnd = i + Math.Min(chunkSize, Points.Length - i);
                var length = indexEnd - indexStart;

                var r = new ReconstructionContainer(this.Width, this.Height, length)
                {
                    IndexStart = indexStart,
                    IndexEnd = indexEnd
                };

                Array.Copy(Points, i, r.Points, 0, length);

                using (var file = File.Create(filename.Replace(".jpg", "."+j + ".bin")))
                {
                    Serializer.Serialize(file, r);
                }
            });
        }
    }
}