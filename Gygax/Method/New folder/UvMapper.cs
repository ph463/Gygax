using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using GygaxCore.DataStructures;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Point = System.Drawing.Point;

namespace GygaxVisu
{
    public class UvMapper
    {

        private readonly MeshGeometry3D _geometry;
        private readonly IfcMeshGeometryModel3D[] _model;

        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }

        public string TextureFilename;

        public int Resolution = 200;

        public UvMapper(MeshGeometry3D geometry, IfcMeshGeometryModel3D[] model)
        {
            _geometry = geometry;
            _model = model;

            Triangle.Triangles = new List<Triangle>();
        }

        public Vector2Collection GenerateAdvancedIndexes()
        {
            var textureMap = 0;
            
            for (int i = 0; i < _geometry.Indices.Count; i += 3)
            {
                var pos1 = _geometry.Positions[_geometry.Indices[i + 0]];
                var pos2 = _geometry.Positions[_geometry.Indices[i + 1]];
                var pos3 = _geometry.Positions[_geometry.Indices[i + 2]];

                Triangles.Add(new Triangle(pos1, pos2, pos3, i));
            }

            while (Triangles.Count(t => !t.Visited) > 0)
            {
                Triangle.MapIt(Triangles.Where(t => !t.Visited).OrderByDescending(t => t.Area).ToList()[0], textureMap);
                textureMap++;
            }

            //DrawTriangles(Triangles);

            return ConvertToIndices(Triangles);
            
        }

        private Vector2Collection ConvertToIndices(List<Triangle> triangles)
        {
            var vectorCollection = new Vector2Collection();

            var offset = new Vector2();

            foreach (var textureMap in triangles.Select(t => t.TextureMap).Distinct())
            {
                var xMin = double.MaxValue;
                var xMax = double.MinValue;
                var yMin = double.MaxValue;
                var yMax = double.MinValue;

                foreach (var triangle in triangles.Where(t => t.TextureMap == textureMap))
                {
                    foreach (var coords in triangle.Corners)
                    {
                        if (coords.CoordinatesTexture.X < xMin) xMin = coords.CoordinatesTexture.X;
                        if (coords.CoordinatesTexture.X > xMax) xMax = coords.CoordinatesTexture.X;
                        if (coords.CoordinatesTexture.Y < yMin) yMin = coords.CoordinatesTexture.Y;
                        if (coords.CoordinatesTexture.Y > yMax) yMax = coords.CoordinatesTexture.Y;
                    }
                }

                double width = xMax - xMin;
                double height = yMax - yMin;

                TextureWidth += Convert.ToInt32(Math.Ceiling(width * Resolution));
                TextureHeight += Convert.ToInt32(Math.Ceiling(height * Resolution));
                
                foreach (var triangle in triangles.Where(t => t.TextureMap == textureMap).OrderBy(t => t.Order))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        vectorCollection.Add(
                            new Vector2(
                                (float)(triangle.Corners[i].CoordinatesTexture.X - xMin + offset.X),
                                (float)(triangle.Corners[i].CoordinatesTexture.Y - yMin)
                                ));
                    }
                }

                offset.X += (float)width;
                offset.Y = Math.Max(offset.Y, (float)height);
            }

            for (int i = 0; i < vectorCollection.Count; i++)
            {
                vectorCollection[i] = new Vector2(vectorCollection[i].X/offset.X, vectorCollection[i].Y/offset.Y);
            }

            var j = 0;
            foreach (var triangle in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    triangle.Corners[i].CoordinatesTexture = new Vector2(
                        vectorCollection[j*3 + i].X * TextureWidth, vectorCollection[j * 3 + i].Y * TextureHeight
                        );
                }

                j++;
            }

            return vectorCollection;
        }

        public void DrawTriangles(List<Triangle> triangles)
        {
            var xMin = double.MaxValue;
            var xMax = double.MinValue;
            var yMin = double.MaxValue;
            var yMax = double.MinValue;

            foreach (var triangle in triangles)
            {
                foreach (var coords in triangle.Corners)
                {
                    if (coords.CoordinatesTexture.X < xMin) xMin = coords.CoordinatesTexture.X;
                    if (coords.CoordinatesTexture.X > xMax) xMax = coords.CoordinatesTexture.X;
                    if (coords.CoordinatesTexture.Y < yMin) yMin = coords.CoordinatesTexture.Y;
                    if (coords.CoordinatesTexture.Y > yMax) yMax = coords.CoordinatesTexture.Y;
                }
            }

            double width = xMax - xMin;
            double height = yMax - yMin;
            
            double scale = 1;

            Image<Bgr, Byte> image = new Image<Bgr, Byte>((int)Math.Ceiling(width*scale)+10, (int)Math.Ceiling(height*scale)+10);

            int p = 0;

            foreach (var triangle in triangles.OrderByDescending(t => t.Area).ToList())
            {
                p++;

                for (int i = 0; i < 3; i++)
                {
                    image.Draw(
                        new LineSegment2D(
                            new Point(
                                (int)(Math.Round((triangle.Corners[i].CoordinatesTexture.X - xMin) * scale)),
                                (int)(Math.Round((triangle.Corners[i].CoordinatesTexture.Y - yMin) * scale))
                                ),
                            new Point(
                                (int)(Math.Round((triangle.Corners[(i + 1) % 3].CoordinatesTexture.X - xMin)* scale)),
                                (int)(Math.Round((triangle.Corners[(i + 1) % 3].CoordinatesTexture.Y - yMin) * scale))
                                )),
                        new Bgr(0, 255*(p-1), 255 * (2-p)),
                        1);
                }
            }

            int j = 0;

            while(File.Exists(@"C:\Users\Philipp\Desktop\textureOutline\Triangles" + j+".png"))
            {
                j++;
            }

            image.Save(@"C:\Users\Philipp\Desktop\textureOutline\Triangles" + j + ".png");


        }

        public Vector2Collection GenerateIndexes()
        {
            if (true)
            {
                return GenerateAdvancedIndexes();
            }
            else
            {
                return GenerateSimpleIndexes();
            }
        }

        public List<Triangle> Triangles = new List<Triangle>();

        public Vector2Collection GenerateSimpleIndexes()
        {
            var vectorCollection = new Vector2Collection();

            var offsetX = 0;

            //List<Triangle> Triangles = new List<Triangle>();

            for (int i = 0; i < _geometry.Indices.Count; i += 3)
            {
                var pos1 = _geometry.Positions[_geometry.Indices[i + 0]];
                var pos2 = _geometry.Positions[_geometry.Indices[i + 1]];
                var pos3 = _geometry.Positions[_geometry.Indices[i + 2]];

                var lengthA = (pos2 - pos1).Length();
                var lengthB = (pos3 - pos1).Length();

                var alpha = Math.Acos(Vector3.Dot(pos2 - pos1, pos3 - pos1) / (lengthA * lengthB));

                var texturePos1 = new Vector2(offsetX, 0);
                var texturePos2 = new Vector2(offsetX, (float) lengthA*Resolution);
                var texturePos3 = new Vector2(offsetX + (float) (Math.Sin(alpha)*lengthB*Resolution),
                    (float) (Math.Cos(alpha)*lengthB*Resolution));

                //vectorCollection.Add(new Vector2(offsetX, 0));
                //vectorCollection.Add(new Vector2(offsetX, (float)lengthA * Resolution));
                //vectorCollection.Add(new Vector2(offsetX + (float)(Math.Sin(alpha) * lengthB * Resolution), (float)(Math.Cos(alpha) * lengthB * Resolution)));

                vectorCollection.Add(texturePos1);
                vectorCollection.Add(texturePos2);
                vectorCollection.Add(texturePos3);

                Triangles.Add(new Triangle(pos1, pos2, pos3, i)
                {
                    Corners = new List<CornerPoint>()
                    {
                        new CornerPoint
                        {
                            CoordinatesTexture = texturePos1,
                            Coordinates3D = pos1
                        },
                        new CornerPoint
                        {
                            CoordinatesTexture = texturePos2,
                            Coordinates3D = pos2
                        },
                        new CornerPoint
                        {
                            CoordinatesTexture = texturePos3,
                            Coordinates3D = pos3
                        }
                    }
                });

                offsetX += (int) (Math.Ceiling(Math.Sin(alpha)*lengthB*Resolution));
            }

            TextureWidth = offsetX + 1;
            TextureHeight = Convert.ToInt32(vectorCollection.Max(v => v.Y)) + 1;

            if (TextureWidth < 1)
                TextureWidth = 1;

            for (int i = 0; i < vectorCollection.Count; i++)
            {
                var v2 = vectorCollection[i];
                vectorCollection[i] = new Vector2(v2.X / TextureWidth, v2.Y / TextureHeight);
            }

            //DrawTriangles(Triangles);

            return vectorCollection;
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

        private delegate Bgr DoAfterGetAddresses(int x, int y);

        public void GenerateSurfaceImageryFromCameraList(List<CameraPosition> cameraPositions, List<Triangle> triangles, bool generateTextureFile = true, bool generateMaskFile = false)
        {
            var projectionBase = new PlaneReconstructor.ProjectionBase();
            
            var planeReconstructor = new PlaneReconstructor(cameraPositions, projectionBase, null, _model)
            {
                GenerateMaskFile = generateMaskFile,
                GenerateTextureFile = generateTextureFile
            };

            planeReconstructor.Triangles = triangles;
            
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(TextureWidth, TextureHeight);
            
            foreach (var triangle in triangles)
            {
                planeReconstructor.ReconstructRayTracer(
                    ref image,
                    triangle,
                    TextureWidth,
                    TextureHeight
                    );
            }

            if(generateMaskFile)
                planeReconstructor.SaveMasks();

            if (generateTextureFile)
                image.Save(TextureFilename);

            Debug.WriteLine(Path.GetFileName(TextureFilename) + " " + planeReconstructor.Stopwatch.Elapsed);
        }

        public void GenerateSurfaceImageryColorPattern2()
        {
            var lut = new Dictionary<int, Bgr>
            {
                {0, new Bgr(255, 0, 0)},
                {1, new Bgr(0, 255, 0)},
                {2, new Bgr(0, 0, 255)},
                {3, new Bgr(0, 255, 255)}
            };

            Image<Bgr, Byte> image = new Image<Bgr, Byte>(TextureWidth, TextureHeight);

            for (int i = 0; i < _geometry.TextureCoordinates.Count; i += 3)
            {
                var pos1 = new System.Drawing.Point((int)(_geometry.TextureCoordinates[i].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i].Y * TextureHeight));
                var pos2 = new System.Drawing.Point((int)(_geometry.TextureCoordinates[i + 1].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i + 1].Y * TextureHeight));
                var pos3 = new Point((int)(_geometry.TextureCoordinates[i + 2].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i + 2].Y * TextureHeight));

                for (int y = pos1.Y; y < Math.Max(pos2.Y, pos3.Y); y++)
                {
                    for (int x = pos1.X; x < pos3.X; x++)
                    {
                        if (
                            !PointInTriangle(new Vector2(x, y), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y),
                                new Vector2(pos3.X, pos3.Y))
                            &&
                            !PointInTriangle(new Vector2(x+1, y), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y),
                                new Vector2(pos3.X, pos3.Y))
                            &&
                            !PointInTriangle(new Vector2(x-1, y), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y),
                                new Vector2(pos3.X, pos3.Y))
                            &&
                            !PointInTriangle(new Vector2(x, y+1), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y),
                                new Vector2(pos3.X, pos3.Y))
                            &&
                            !PointInTriangle(new Vector2(x, y-1), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y),
                                new Vector2(pos3.X, pos3.Y))
                                )
                        {
                            image[y, x] = new Bgr(0,0,255);
                            continue;
                        }

                        if (!(y < image.Height && x < image.Width))
                            continue;

                        image[y, x] = new Bgr(0,255,0);
                    }

                }
            }

            image.Save(TextureFilename);
        }

        public void GenerateSurfaceImageryColorPattern()
        {
            //var lut = new Dictionary<int, Bgr>
            //{
            //    {0, new Bgr(255, 0, 0)},
            //    {1, new Bgr(0, 255, 0)},
            //    {2, new Bgr(0, 0, 255)},
            //    {3, new Bgr(0, 255, 255)}
            //};

            var lut = new Dictionary<int, Bgr>
            {
                {0, new Bgr(255, 255, 255)},
                {1, new Bgr(255, 255, 255)},
                {2, new Bgr(255, 255, 255)},
                {3, new Bgr(255, 255, 255)},
                {4, new Bgr(0, 0, 0)},
                {5, new Bgr(0, 0, 0)},
                {6, new Bgr(0, 0, 0)},
                {7, new Bgr(0, 0, 0)}
            };

            IterateTexture((x, y) => lut[(x + y)%lut.Count]);
        }

        private void IterateTexture(DoAfterGetAddresses doit)
        {
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(TextureWidth, TextureHeight);

            for (int i = 0; i < _geometry.TextureCoordinates.Count; i += 3)
            {
                var pos1 = new System.Drawing.Point((int)(_geometry.TextureCoordinates[i].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i].Y * TextureHeight));
                var pos2 = new System.Drawing.Point((int)(_geometry.TextureCoordinates[i + 1].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i + 1].Y * TextureHeight));
                var pos3 = new Point((int)(_geometry.TextureCoordinates[i + 2].X * TextureWidth),
                    (int)(_geometry.TextureCoordinates[i + 2].Y * TextureHeight));
                
                for (int y = pos1.Y; y < Math.Max(pos2.Y, pos3.Y); y++)
                {
                    for (int x = pos1.X; x < pos3.X; x++)
                    {
                        if (!PointInTriangle(new Vector2(x, y), new Vector2(pos1.X, pos1.Y), new Vector2(pos2.X, pos2.Y), new Vector2(pos3.X, pos3.Y)))
                            continue;

                        if (!(y < image.Height && x < image.Width))
                            continue;

                        image[y, x] = doit(x, y);
                    }

                }
            }
            
            image.Save(TextureFilename);
        }
    }

}
