using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GygaxCore.DataStructures;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace GygaxVisu
{
    public class Triangle
    {
        public List<CornerPoint> Corners = new List<CornerPoint>();

        public static List<Triangle> Triangles = new List<Triangle>();

        public List<Neighbour> Neighbours = new List<Neighbour>();

        //public List<CameraPosition> VisibleCameras = new List<CameraPosition>();

        public Dictionary<CameraPosition, List<Triangle>> TrianglesToConsider = new Dictionary<CameraPosition, List<Triangle>>();

        public bool Visited;

        public readonly int Order;

        public int TextureMap;

        public double Area
        {
            get
            {
                var a = (Corners[1].Coordinates3D - Corners[0].Coordinates3D).Length();
                var b = (Corners[2].Coordinates3D - Corners[1].Coordinates3D).Length();
                var c = (Corners[0].Coordinates3D - Corners[2].Coordinates3D).Length();
                var s = (a + b + c) / 2;
                return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
            }
        }
        
        private struct CommonEdge
        {
            public Triangle triangle;
            public int i;
            public int j;
            public bool reverse;
            public double length;
        }
        
        public Triangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, int order)
        {
            Order = order;
            Corners.Add(new CornerPoint { Coordinates3D = pos1 });
            Corners.Add(new CornerPoint { Coordinates3D = pos2 });
            Corners.Add(new CornerPoint { Coordinates3D = pos3 });

            CheckForNeighbours();

            Triangles.Add(this);

        }

        public static void MapIt(Triangle t, int textureMap)
        {
            if (t.Visited) return;

            var orderedNeighbors = t.Neighbours.Where(n => n.Triangle.Visited).OrderByDescending(n => n.LengthOfConnection).ToList();

            if (orderedNeighbors.Count == 0)
            {
                t.SetTextureCoordinates();
            }
            else
            {
                t.SetTextureCoordinates(orderedNeighbors[0].Triangle);
            }

            t.TextureMap = textureMap;
            t.Visited = true;

            foreach (var n in t.Neighbours.Where(n => n.Triangle.Visited == false).OrderByDescending(n => n.LengthOfConnection).ToList())
            {
                MapIt(n.Triangle, textureMap);
            }
        }

        public void CheckForNeighbours()
        {
            foreach (var triangle in Triangles)
            {
                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (triangle.Corners[i].Coordinates3D == Corners[j].Coordinates3D &&
                            triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D)
                        {
                            var length =
                                (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length();

                            Neighbours.Add(new Neighbour() { Triangle = triangle, LengthOfConnection = length });

                            triangle.Neighbours.Add(new Neighbour() { Triangle = this, LengthOfConnection = length });
                        }

                        if (triangle.Corners[i].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D &&
                            triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[j].Coordinates3D)
                        {
                            var length =
                                (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length();

                            Neighbours.Add(new Neighbour() { Triangle = triangle, LengthOfConnection = length });

                            triangle.Neighbours.Add(new Neighbour() { Triangle = this, LengthOfConnection = length });
                        }
                    }
                }

            }
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

                continue;

                
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

        public double GetClosestCornerDistance(Vector3 point)
        {
            return (GetClosestCorner(point).Coordinates3D - point).Length();
        }

        public CornerPoint GetClosestCorner(Vector3 point)
        {
            var minValue = double.MaxValue;
            CornerPoint minObject = null;

            foreach (var cornerPoint in Corners)
            {
                if ((cornerPoint.Coordinates3D - point).LengthSquared() < minValue)
                {
                    minValue = (cornerPoint.Coordinates3D - point).LengthSquared();
                    minObject = cornerPoint;
                }
            }

            return minObject;
        }

        public double GetFarthestCornerDistance(Vector3 point)
        {
            return (GetFarthestCorner(point).Coordinates3D - point).Length();
        }

        public CornerPoint GetFarthestCorner(Vector3 point)
        {
            var maxValue = double.MinValue;
            CornerPoint maxObject = null;

            foreach (var cornerPoint in Corners)
            {
                if ((cornerPoint.Coordinates3D - point).LengthSquared() > maxValue)
                {
                    maxValue = (cornerPoint.Coordinates3D - point).LengthSquared();
                    maxObject = cornerPoint;
                }
            }

            return maxObject;
        }

        public void SetTextureCoordinates()
        {
            Corners[0].CoordinatesTexture = new Vector2(0, 0);
            Corners[1].CoordinatesTexture = new Vector2(0, (Corners[1].Coordinates3D - Corners[0].Coordinates3D).Length());
            Corners[2] = SetCoordinatePoints(Corners[0], Corners[1], Corners[2]);
        }

        public void SetTextureCoordinates(Triangle triangle)
        {
            var commonEdges = new List<CommonEdge>();
            
            //Check for longest common edge
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (triangle.Corners[i].Coordinates3D == Corners[j].Coordinates3D && triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D)
                    {
                        commonEdges.Add(new CommonEdge
                        {
                            triangle = triangle,
                            i = i,
                            j = j,
                            reverse = false,
                            length = (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length()
                        });
                    }

                    if (triangle.Corners[i].Coordinates3D == Corners[(j + 1) % 3].Coordinates3D && triangle.Corners[(i + 1) % 3].Coordinates3D == Corners[j].Coordinates3D)
                    {
                        commonEdges.Add(new CommonEdge
                        {
                            triangle = triangle,
                            i = i,
                            j = j,
                            reverse = true,
                            length = (triangle.Corners[i].Coordinates3D - triangle.Corners[(i + 1) % 3].Coordinates3D).Length()
                        });
                    }
                }
            }

            if (commonEdges.Count == 0)
                return;

            var longestEdge = commonEdges.OrderByDescending(edge => edge.length).ToList()[0];

            Corners[(longestEdge.j + 1) % 3].CoordinatesTexture = longestEdge.triangle.Corners[longestEdge.i].CoordinatesTexture;
            Corners[longestEdge.j].CoordinatesTexture = longestEdge.triangle.Corners[(longestEdge.i + 1) % 3].CoordinatesTexture;

            if (!longestEdge.reverse)
            {
                Corners[(longestEdge.j + 2) % 3] = SetCoordinatePoints(Corners[longestEdge.j], Corners[(longestEdge.j + 1) % 3], Corners[(longestEdge.j + 2) % 3]);
            }
            else
            {
                Corners[(longestEdge.j + 2) % 3] = SetCoordinatePoints(Corners[(longestEdge.j + 1) % 3], Corners[longestEdge.j], Corners[(longestEdge.j + 2) % 3], true);
            }
        }

        public CornerPoint SetCoordinatePoints(CornerPoint commonCorner, CornerPoint setCorner, CornerPoint unsetCorner, bool reverse = false)
        {
            var a = GetAngle(setCorner.Coordinates3D, commonCorner.Coordinates3D, unsetCorner.Coordinates3D);

            if (!reverse) a = -a;

            var l = (commonCorner.Coordinates3D - unsetCorner.Coordinates3D).Length();

            var z = setCorner.CoordinatesTexture - commonCorner.CoordinatesTexture;
            z.Normalize();
            z *= l;

            unsetCorner.CoordinatesTexture = commonCorner.CoordinatesTexture + new Vector2(
                (float)(z.X * Math.Cos(a) - z.Y * Math.Sin(a)),
                (float)(z.X * Math.Sin(a) + z.Y * Math.Cos(a))
                );

            return unsetCorner;
        }

        public static double GetAngle(Vector3 point1, Vector3 commonPoint, Vector3 point2)
        {
            var a = Vector3.Dot(
                point1 - commonPoint,
                point2 - commonPoint
                );

            var b = a / ((point1 - commonPoint).Length() * (point2 - commonPoint).Length());

            return Math.Acos(b);
        }
    }
    
    public class Neighbour
    {
        public Triangle Triangle;

        public double LengthOfConnection;

    }

    public class CornerPoint
    {
        public Vector3 Coordinates3D;

        private Vector2 _coordinatesTexture;

        public Vector2 CoordinatesTexture
        {
            get { return _coordinatesTexture; }
            set
            {
                if (!double.IsNaN(value.X) && !double.IsNaN(value.Y))
                    TextureCoordinatesSet = true;

                _coordinatesTexture = value;
            }
        }

        public bool TextureCoordinatesSet;
    }
}
