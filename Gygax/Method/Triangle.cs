using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace GygaxVisu
{
    public class Triangle
    {
        //public List<CornerPoint> Corners = new List<CornerPoint>();

        public CornerPoint[] Corners => ActiveConfiguration.Corners;
        
        public Configuration _activeConfiguration = new Configuration();

        public Configuration ActiveConfiguration
        {
            get
            {
                return _activeConfiguration;
            }
            set
            {
                _activeConfiguration = value;
                Normal = new Plane(value.Corners[0].Coordinates3D.ToVector3(), value.Corners[1].Coordinates3D.ToVector3(), value.Corners[2].Coordinates3D.ToVector3()).Normal.Normalized();
            }
        }

        public static List<Triangle> Triangles = new List<Triangle>();

        public List<CommonEdge> Neighbours = new List<CommonEdge>();

        public Dictionary<CameraPosition, List<Triangle>> TrianglesToConsider = new Dictionary<CameraPosition, List<Triangle>>();

        public bool IsSetout => ActiveConfiguration.Corners.All(corner => corner.IsSetout);

        public readonly int Order;

        public int TextureMap;

        public Vector3 Normal { get; private set; }

        public double Area
        {
            get
            {
                var a = (ActiveConfiguration.Corners[1].Coordinates3D - ActiveConfiguration.Corners[0].Coordinates3D).Length;
                var b = (ActiveConfiguration.Corners[2].Coordinates3D - ActiveConfiguration.Corners[1].Coordinates3D).Length;
                var c = (ActiveConfiguration.Corners[0].Coordinates3D - ActiveConfiguration.Corners[2].Coordinates3D).Length;
                var s = (a + b + c) / 2;
                return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
            }
        }

        public struct CommonEdge
        {
            public Triangle Triangle;
            public int[,] ConnectionIndex;

            public double Length => (Triangle.ActiveConfiguration.Corners[ConnectionIndex[1,0]].Coordinates3D - Triangle.ActiveConfiguration.Corners[ConnectionIndex[1, 1]].Coordinates3D).Length;
        }
        
        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, int order)
        {
            Order = order;
            
            ActiveConfiguration.Corners[0] = new CornerPoint { Coordinates3D = new Vector3D(pos1.X, pos1.Y, pos1.Z) };
            ActiveConfiguration.Corners[1] = new CornerPoint { Coordinates3D = new Vector3D(pos2.X, pos2.Y, pos2.Z) };
            ActiveConfiguration.Corners[2] = new CornerPoint { Coordinates3D = new Vector3D(pos3.X, pos3.Y, pos3.Z) };

            CheckForNeighbours();

            Triangles.Add(this);

        }

        public static Triangle GetInitialUnsetTriangle(int textureMap)
        {
            var t = Triangles.Where(c => !c.IsSetout).Where(c => c.Area > 0.0).OrderBy(c => c.Area).First();
            t.TextureMap = textureMap;
            return t;
        }

        //public void MapIt()
        //{
        //    switch (Neighbours.Count(neighbour => neighbour.Triangle.IsSetout))
        //    {
        //        default:
        //            {
        //                throw new Exception("Triangle has " + Neighbours.Count(neighbour => neighbour.Triangle.IsSetout) + " mapped members");
        //            }
        //        case 0:
        //            {
        //                SetTextureCoordinates();
        //                break;
        //            }
        //        case 1:
        //            {
        //                break;
        //            }
        //        case 2:
        //            {
        //                break;
        //            }
        //    }
        //}

        public void CheckForNeighbours()
        {
            foreach (var triangle in Triangles)
            {
                if (HasCommonEdge(triangle))
                {
                    Neighbours.Add(new CommonEdge
                    {
                        Triangle = triangle,
                        ConnectionIndex = GetCommonEdge(triangle)
                    });
                    triangle.Neighbours.Add(new CommonEdge
                    {
                        Triangle = this,
                        ConnectionIndex = triangle.GetCommonEdge(this)
                    });
                }
            }
        }

        public bool HasCommonEdge(Triangle triangle)
        {
            var m = GetCommonEdge(triangle);

            if (m[0, 0] + m[0, 1] + m[1, 0] + m[1, 1] == 0)
                return false;

            return true;
        }

        public int[,] GetCommonEdge(Triangle triangle)
        {
            var returnList = new int[2,2];

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (triangle.ActiveConfiguration.Corners[i].Coordinates3D == ActiveConfiguration.Corners[j].Coordinates3D &&
                        triangle.ActiveConfiguration.Corners[(i + 1) % 3].Coordinates3D == ActiveConfiguration.Corners[(j + 1) % 3].Coordinates3D)
                    {
                        returnList[0,0] = j;
                        returnList[0,1] = (j + 1) % 3;
                        returnList[1,0] = i;
                        returnList[1,1] = (i + 1) % 3;
                    }

                    if (triangle.ActiveConfiguration.Corners[i].Coordinates3D == ActiveConfiguration.Corners[(j + 1) % 3].Coordinates3D &&
                        triangle.ActiveConfiguration.Corners[(i + 1) % 3].Coordinates3D == ActiveConfiguration.Corners[j].Coordinates3D)
                    {
                        returnList[0,0] = (j + 1) % 3;
                        returnList[0,1] = j;
                        returnList[1,0] = i;
                        returnList[1,1] = (i + 1) % 3;
                    }
                }
            }

            return returnList;
        }

        public void CalculateVisibility(List<CameraPosition> cameraPositions, List<Triangle> triangles)
        {
            foreach (var cameraPosition in cameraPositions)
            {

                TrianglesToConsider.Add(cameraPosition, new List<Triangle>());

                foreach (var triangle in triangles)
                {
                    if (triangle.Equals(this))
                        continue;

                    TrianglesToConsider[cameraPosition].Add(triangle);
                }

                //continue;


                //var blocked = false;

                //foreach (var triangle in triangles)
                //{
                //    //same object
                //    if (triangle.Equals(this))
                //        continue;

                //    //all corner points further and all hit closer triangle (is triangle visible from this camera at all
                //    //this is the only condition that also excludes all other triangles
                //    var ray1 = new Ray(cameraPosition.CameraCenter, (Corners[0].Coordinates3D - cameraPosition.CameraCenter));
                //    var ray2 = new Ray(cameraPosition.CameraCenter, (Corners[1].Coordinates3D - cameraPosition.CameraCenter));
                //    var ray3 = new Ray(cameraPosition.CameraCenter, (Corners[2].Coordinates3D - cameraPosition.CameraCenter));

                //    Vector3 intersection;

                //    if (
                //        Collision.RayIntersectsTriangle(ref ray1, ref triangle.Corners[0].Coordinates3D, ref triangle.Corners[1].Coordinates3D, ref triangle.Corners[2].Coordinates3D, out intersection)
                //        && Collision.RayIntersectsTriangle(ref ray2, ref triangle.Corners[0].Coordinates3D, ref triangle.Corners[1].Coordinates3D, ref triangle.Corners[2].Coordinates3D, out intersection)
                //        && Collision.RayIntersectsTriangle(ref ray3, ref triangle.Corners[0].Coordinates3D, ref triangle.Corners[1].Coordinates3D, ref triangle.Corners[2].Coordinates3D, out intersection)
                //        )
                //    {
                //        blocked = true;
                //        break;
                //    }
                //}
                
                //if (!blocked)
                //{
                //    TrianglesToConsider.Add(cameraPosition, new List<Triangle>());

                //    foreach (var triangle in triangles)
                //    {
                //        //same object
                //        if (triangle.Equals(this))
                //            continue;

                //        //other triangle completely behind this triangle
                //        if (triangle.GetClosestCornerDistance(cameraPosition.CameraCenter) >
                //            GetFarthestCornerDistance(cameraPosition.CameraCenter))
                //            continue;

                //        //one corner of other triangle hits this triangle
                //        float Distance;
                //        var foundOne = false;

                //        for (int i = 0; i < 3; i++)
                //        {
                //            var r = new Ray(cameraPosition.CameraCenter, (triangle.Corners[i].Coordinates3D - cameraPosition.CameraCenter).Normalized());
                //            if (
                //                Collision.RayIntersectsTriangle(
                //                    ref r,
                //                    ref Corners[0].Coordinates3D,
                //                    ref Corners[1].Coordinates3D,
                //                    ref Corners[2].Coordinates3D,
                //                    out Distance) &&
                //                Distance * Distance <
                //                (cameraPosition.CameraCenter - triangle.Corners[i].Coordinates3D).LengthSquared())
                //            {
                //                foundOne = true;
                //                break;
                //            }
                //        }

                //        if (foundOne)
                //        {
                //            TrianglesToConsider[cameraPosition].Add(triangle);
                //            continue;
                //        }

                //        //other triangle collides with triangle made of cameracenter and one triangle edge
                //        var t1Collision = TriangleIntersectsTriangle(
                //            ref Corners[0].Coordinates3D,
                //            ref Corners[1].Coordinates3D,
                //            ref cameraPosition.CameraCenter,
                //            ref triangle.Corners[0].Coordinates3D,
                //            ref triangle.Corners[1].Coordinates3D,
                //            ref triangle.Corners[2].Coordinates3D);

                //        var t2Collision = TriangleIntersectsTriangle(
                //            ref Corners[1].Coordinates3D,
                //            ref Corners[2].Coordinates3D,
                //            ref cameraPosition.CameraCenter,
                //            ref triangle.Corners[0].Coordinates3D,
                //            ref triangle.Corners[1].Coordinates3D,
                //            ref triangle.Corners[2].Coordinates3D);

                //        var t3Collision = TriangleIntersectsTriangle(
                //            ref Corners[2].Coordinates3D,
                //            ref Corners[0].Coordinates3D,
                //            ref cameraPosition.CameraCenter,
                //            ref triangle.Corners[0].Coordinates3D,
                //            ref triangle.Corners[1].Coordinates3D,
                //            ref triangle.Corners[2].Coordinates3D);


                //        if (t1Collision || t2Collision || t3Collision)
                //        {
                //            TrianglesToConsider[cameraPosition].Add(triangle);
                //        }
                //    }
                //}
            }
        }

        public static bool TriangleIntersectsTriangle(ref Vector3 vertex11, ref Vector3 vertex12, ref Vector3 vertex13,
            ref Vector3 vertex21, ref Vector3 vertex22, ref Vector3 vertex23)
        {
            float distance;

            var r = new Ray(vertex11, (vertex12-vertex11).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex21,
                ref vertex22,
                ref vertex23,
                out distance
                )
                &&
                distance * distance < (vertex12 - vertex11).LengthSquared()
                )
            {
                return true;
            }
                

            r = new Ray(vertex11, (vertex13 - vertex11).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex21,
                ref vertex22,
                ref vertex23,
                out distance
                )
                &&
                distance * distance < (vertex13 - vertex11).LengthSquared()
                )
            {
                return true;
            }

            r = new Ray(vertex12, (vertex13 - vertex12).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex21,
                ref vertex22,
                ref vertex23,
                out distance
                )
                &&
                distance * distance < (vertex13 - vertex12).LengthSquared()
                )
            {
                return true;
            }

            r = new Ray(vertex21, (vertex22 - vertex21).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex11,
                ref vertex12,
                ref vertex13,
                out distance
                )
                &&
                distance * distance < (vertex22 - vertex21).LengthSquared()
                )
            {
                return true;
            }

            r = new Ray(vertex21, (vertex23 - vertex21).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex11,
                ref vertex12,
                ref vertex13,
                out distance
                )
                &&
                distance * distance < (vertex23 - vertex21).LengthSquared()
                )
            {
                return true;
            }

            r = new Ray(vertex22, (vertex23 - vertex22).Normalized());

            if (Collision.RayIntersectsTriangle(
                ref r,
                ref vertex11,
                ref vertex12,
                ref vertex13,
                out distance
                )
                &&
                distance * distance < (vertex23 - vertex22).LengthSquared()
                )
            {
                return true;
            }

            return false;
        }

        public double GetClosestCornerDistance(Vector3D point)
        {
            return (GetClosestCorner(point).Coordinates3D - point).Length;
        }

        public CornerPoint GetClosestCorner(Vector3D point)
        {
            var minValue = double.MaxValue;
            CornerPoint minObject = new CornerPoint();

            foreach (var cornerPoint in ActiveConfiguration.Corners)
            {
                if ((cornerPoint.Coordinates3D - point).Length < minValue)
                {
                    minValue = (cornerPoint.Coordinates3D - point).Length;
                    minObject = cornerPoint;
                }
            }

            return minObject;
        }

        public double GetFarthestCornerDistance(Vector3D point)
        {
            return (GetFarthestCorner(point).Coordinates3D - point).Length;
        }

        public CornerPoint GetFarthestCorner(Vector3D point)
        {
            var maxValue = double.MinValue;
            CornerPoint maxObject = new CornerPoint();

            foreach (var cornerPoint in ActiveConfiguration.Corners)
            {
                if ((cornerPoint.Coordinates3D - point).Length > maxValue)
                {
                    maxValue = (cornerPoint.Coordinates3D - point).Length;
                    maxObject = cornerPoint;
                }
            }

            return maxObject;
        }

        public void SetTextureCoordinates()
        {
            //Corners[0].CoordinatesTexture = new Vector2(0, 0);
            //Corners[1].CoordinatesTexture = new Vector2(0, (Corners[1].Coordinates3D - Corners[0].Coordinates3D).Length());
            //Corners[2] = SetCoordinatePoints(Corners[0], Corners[1], Corners[2]);
        }

        public void SetTextureCoordinates(Triangle triangle)
        {

            //var commonEdges = new List<CommonEdge>();
            
            ////Check for longest common edge
            //for (var i = 0; i < 3; i++)
            //{
            //    for (var j = 0; j < 3; j++)
            //    {
            //        if (triangle.Corners[i].Coordinates3D == Corners[j].Coordinates3D && triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D)
            //        {
            //            commonEdges.Add(new CommonEdge
            //            {
            //                triangle = triangle,
            //                i = i,
            //                j = j,
            //                reverse = false,
            //                length = (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length()
            //            });
            //        }

            //        if (triangle.Corners[i].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D && triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[j].Coordinates3D)
            //        {
            //            commonEdges.Add(new CommonEdge
            //            {
            //                triangle = triangle,
            //                i = i,
            //                j = j,
            //                reverse = true,
            //                length = (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length()
            //            });
            //        }
            //    }
            //}

            //if (commonEdges.Count == 0)
            //    return;

            //var longestEdge = commonEdges.OrderByDescending(edge => edge.length).ToList()[0];

            //Corners[(longestEdge.j + 1) % 3].CoordinatesTexture = longestEdge.triangle.Corners[longestEdge.i].CoordinatesTexture;
            //Corners[longestEdge.j].CoordinatesTexture = longestEdge.triangle.Corners[(longestEdge.i + 1) % 3].CoordinatesTexture;

            //if (!longestEdge.reverse)
            //{
            //    Corners[(longestEdge.j + 2) % 3] = SetCoordinatePoints(Corners[longestEdge.j], Corners[(longestEdge.j + 1) % 3], Corners[(longestEdge.j + 2) % 3]);
            //}
            //else
            //{
            //    Corners[(longestEdge.j + 2) % 3] = SetCoordinatePoints(Corners[(longestEdge.j + 1) % 3], Corners[longestEdge.j], Corners[(longestEdge.j + 2) % 3], true);
            //}
        }

        public void PickBestConfiguration()
        {
            ActiveConfiguration = GetBestConfiguration();
        }

        public Configuration GetBestConfiguration()
        {
            var list = GetSetoutConfiguration();

            var max = list.Max(c => c.PossibleSides);
            
            return list.Where(c => c.PossibleSides == max).OrderBy(c => c.Increase).First();
        }

        public class Configuration
        {
            public CornerPoint[] Corners = new CornerPoint[3];
            public double Increase;

            public double Length(int index) => (Corners[(index + 1)%3].Coordinates3D - Corners[index%3].Coordinates3D).Length;

            public double LengthOfContour => Length(0) + Length(1) + Length(2);

            public int PossibleSides;

            public Configuration Duplicate()
            {
                var c = new Configuration();

                c.Corners = new CornerPoint[3];

                for (int i = 0; i < 3; i++)
                {
                    c.Corners[i] = new CornerPoint()
                    {
                        CoordinatesTexture = Corners[i].CoordinatesTexture,
                        Coordinates3D = Corners[i].Coordinates3D
                    };
                }

                c.Increase = Increase;

                c.PossibleSides = PossibleSides;

                return c;
            }
        }

        public List<Configuration> GetSetoutConfiguration()
        {
            var configurations = new List<Configuration>();

            switch (Neighbours.Count(neighbour => neighbour.Triangle.IsSetout))
            {
                case 0:
                    {
                        var c = ActiveConfiguration.Duplicate();

                        c.Corners[0].CoordinatesTexture = new Vector3D(0,0,0);
                        c.Corners[1].CoordinatesTexture = new Vector3D(0, (ActiveConfiguration.Corners[1].Coordinates3D - ActiveConfiguration.Corners[0].Coordinates3D).Length,0);

                        c.Corners[2] = SetCoordinatePoints(
                            c.Corners[0],
                            c.Corners[1],
                            c.Corners[2]);

                        c.Increase = c.LengthOfContour;

                        c.PossibleSides = 0;

                        configurations.Add(c);
                        
                        break;
                    }
                case 1:
                    {
                        var commonEdge = Neighbours.Where(n => n.Triangle.IsSetout).ElementAt(0);

                        var c = ActiveConfiguration.Duplicate();

                        c.Corners[commonEdge.ConnectionIndex[0, 0]] =
                            commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 0]];
                        c.Corners[commonEdge.ConnectionIndex[0, 1]] =
                            commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 1]];

                        int missingIndex = 3 - commonEdge.ConnectionIndex[0, 0] - commonEdge.ConnectionIndex[0, 1];

                        c.Corners[missingIndex] = SetCoordinatePoints(
                            commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 0]],
                            commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 1]],
                            c.Corners[missingIndex], true);

                        c.Increase = c.LengthOfContour - 2*commonEdge.Length;

                        c.PossibleSides = 1;

                        configurations.Add(c);

                        break;
                    }
                case 5://??
                case 4://??
                case 3:
                case 2:
                    {
                        var setoutList = Neighbours.Where(n => n.Triangle.IsSetout).ToList();

                        for (int i = 0; i < 2; i++)
                        {
                            var commonEdge = setoutList.ElementAt(i);
                            var otherEdge = setoutList.ElementAt((i+1)%2);

                            var c = ActiveConfiguration.Duplicate();

                            c.Corners[commonEdge.ConnectionIndex[0, 0]] =
                                commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 0]];
                            c.Corners[commonEdge.ConnectionIndex[0, 1]] =
                                commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 1]];

                            int missingIndex = 3 - commonEdge.ConnectionIndex[0, 0] - commonEdge.ConnectionIndex[0, 1];

                            c.Corners[missingIndex] = SetCoordinatePoints(
                                commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 0]],
                                commonEdge.Triangle.ActiveConfiguration.Corners[commonEdge.ConnectionIndex[1, 1]],
                                c.Corners[missingIndex], true);

                            var otherEdgeIndex = int.MaxValue;

                            for (int j = 0; j < 2; j++)
                            {
                                if (otherEdge.ConnectionIndex[0, j] == missingIndex)
                                    otherEdgeIndex = otherEdge.ConnectionIndex[1, j];
                            }

                            var distance = (c.Corners[missingIndex].CoordinatesTexture -
                                 otherEdge.Triangle.ActiveConfiguration.Corners[otherEdgeIndex].CoordinatesTexture).Length;

                            //Debug.WriteLine(distance);

                            if (distance < 0.005)
                            {
                                c.Corners[missingIndex].CoordinatesTexture =
                                    otherEdge.Triangle.ActiveConfiguration.Corners[otherEdgeIndex].CoordinatesTexture;
                                c.Increase = c.LengthOfContour - commonEdge.Length - otherEdge.Length;
                                c.PossibleSides = 2;
                                configurations.Add(c);
                            }
                            else
                            {
                                c.Increase = c.LengthOfContour - commonEdge.Length;
                                c.PossibleSides = 1;
                                configurations.Add(c);
                            }
                        }
                        break;
                    }
                //case 3:
                //    {
                //        var setoutList = Neighbours.Where(n => n.Triangle.IsSetout).ToList();




                //        break;
                //    }
            }
            
            if(configurations.Count == 0)
                Debug.WriteLine(Neighbours.Count(neighbour => neighbour.Triangle.IsSetout));

            return configurations;
        }

        public static CornerPoint SetCoordinatePoints(CornerPoint commonCorner, CornerPoint setCorner, CornerPoint unsetCorner, bool reverse = false)
        {
            var a = GetAngle(setCorner.Coordinates3D, commonCorner.Coordinates3D, unsetCorner.Coordinates3D);

            if (!reverse) a = -a;

            var l = (commonCorner.Coordinates3D - unsetCorner.Coordinates3D).Length;

            var z = setCorner.CoordinatesTexture - commonCorner.CoordinatesTexture;
            z.Normalize();
            z *= l;

            unsetCorner.CoordinatesTexture = commonCorner.CoordinatesTexture + new Vector3D(
                (z.X * Math.Cos(a) - z.Y * Math.Sin(a)),
                (z.X * Math.Sin(a) + z.Y * Math.Cos(a)),
                0
                );

            return unsetCorner;
        }

        public static double GetAngle(Vector3D point1, Vector3D commonPoint, Vector3D point2)
        {
            var a = Vector3D.DotProduct(
                point1 - commonPoint,
                point2 - commonPoint
                );

            var b = a / ((point1 - commonPoint).Length * (point2 - commonPoint).Length);

            return Math.Acos(b);
        }

        public static Vector3D FloatToDoubleVector(Vector2 vector)
        {
            return new Vector3D(vector.X, vector.Y, 0);
        }

        public static Vector3D FloatToDoubleVector(Vector3 vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }
    }

    public struct CornerPoint
    {
        public Vector3 Coordinates3DFloat;

        private Vector3D _coordinates3D;// = new Vector3D(double.NaN, double.NaN, double.NaN);

        public Vector3D Coordinates3D
        {
            get
            {
                return _coordinates3D;
            }
            set
            {
                _coordinates3D = value;
                Coordinates3DFloat = new Vector3((float)_coordinates3D.X, (float)_coordinates3D.Y, (float)_coordinates3D.Z);
            }
        }

        public Vector2 CoordinatesTextureFloat { get; private set; }

        //private Vector3D _coordinatesTexture = new Vector3D(double.NaN, double.NaN, 0);
        private Vector3D _coordinatesTexture;

        public Vector3D CoordinatesTexture
        {
            get
            {
                return _coordinatesTexture;
            }
            set
            {
                _coordinatesTexture = value;
                CoordinatesTextureFloat = new Vector2((float)_coordinatesTexture.X, (float)_coordinatesTexture.Y);
                IsSetout = true;
            }
        }

        public bool IsSetout { get; private set; }

        //public bool IsSetout => !double.IsNaN(CoordinatesTexture.X) && !double.IsNaN(CoordinatesTexture.Y);
    }
}
