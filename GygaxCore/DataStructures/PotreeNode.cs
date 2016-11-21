using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using PclWrapper;

namespace GygaxCore.DataStructures
{
    /// <summary>
    /// https://github.com/potree/potree/blob/master/docs/file_format.md
    /// </summary>

    public class PotreeNode
    {
        public class TreeSettings
        {
            public string Version;
            public string OctreeDir;
            public int OverallPointNumber;
            public Rect3D BoundingBox;
            public Rect3D TightBoundingBox;
            public object PointAttributes;
            public double Spacing;
            public double Scale;
            public int HierarchyStepSize;
            public int MaxPointNumber = 1000000;
        }

        public Rect3D BoundingBox;

        public TreeSettings Settings;

        public class TreeCompilation
        {
            public List<Points> Points;

            public TreeCompilation()
            {
                Points = new List<Points>();
            }
        }

        public TreeCompilation Compilation;

        public int NodePointNumber;
        public int Level;
        public PotreeNode[] Children = new PotreeNode[8];
        public byte Mask;
        public string Filename;

        private PotreeNode _next;

        public PotreeNode(Uri location)
        {
            ReadSettings(location);
            Compilation = new TreeCompilation();
            BoundingBox = Settings.BoundingBox;
            Filename = "r";
            BuiltHierarchy();
            UpdateCompilation();
        }

        private PotreeNode(){}
        
        private void ReadSettings(Uri location)
        {
            using (var reader = new StreamReader(location.LocalPath))
            {
                dynamic stuff = JsonConvert.DeserializeObject(reader.ReadToEnd());

                Settings = new TreeSettings
                {
                    Version = stuff.version,
                    OctreeDir = Path.GetDirectoryName(location.LocalPath) + @"\" + stuff.octreeDir + @"\r",
                    OverallPointNumber = stuff.points,

                    BoundingBox = new Rect3D
                    {
                        X = stuff.boundingBox.lx,
                        Y = stuff.boundingBox.lz,
                        Z = stuff.boundingBox.ly,
                        SizeX = stuff.boundingBox.ux - stuff.boundingBox.lx.Value,
                        SizeY = stuff.boundingBox.uz - stuff.boundingBox.lz.Value,
                        SizeZ = stuff.boundingBox.uy - stuff.boundingBox.ly.Value
                    },

                    TightBoundingBox = new Rect3D
                    {
                        X = stuff.tightBoundingBox.lx,
                        Y = stuff.tightBoundingBox.lz,
                        Z = stuff.tightBoundingBox.ly,
                        SizeX = stuff.tightBoundingBox.ux - stuff.tightBoundingBox.lx.Value,
                        SizeY = stuff.tightBoundingBox.uz - stuff.tightBoundingBox.lz.Value,
                        SizeZ = stuff.tightBoundingBox.uy - stuff.tightBoundingBox.ly.Value
                    },

                    Spacing = stuff.spacing,
                    Scale = stuff.scale,
                    HierarchyStepSize = stuff.hierarchyStepSize
                };
            }
        }

        public void BuiltHierarchy()
        {
            // First read masks and point numbers to a queue
            Queue<byte> masks = new Queue<byte>();
            Queue<int> pointNumbers = new Queue<int>();

            using (BinaryReader b = new BinaryReader(File.Open(Settings.OctreeDir + @"\r.hrc", FileMode.Open)))
            {
                int pos = 0;

                int length = (int)b.BaseStream.Length;

                while (pos < length)
                {
                    var mask = b.ReadByte();
                    var numberOfPoints = b.ReadInt32();

                    pos += sizeof(byte) + sizeof(Int32);
                    
                    masks.Enqueue(mask);
                    pointNumbers.Enqueue(numberOfPoints);
                }
            }

            //Now to a breadth-first search to assemble the tree
            Queue<PotreeNode> nodeQueue = new Queue<PotreeNode>();

            nodeQueue.Enqueue(this);

            var previousNode = this;

            while (nodeQueue.Count > 0)
            {
                var node = nodeQueue.Dequeue();
                previousNode._next = node;

                node.Mask = masks.Dequeue();
                node.NodePointNumber = pointNumbers.Dequeue();

                if (node.Level >= Settings.HierarchyStepSize)
                    continue;

                for (int i = 0; i < 8; i++)
                {
                    if ((node.Mask >> i & 0x01) == 1)
                    {
                        var n = new PotreeNode()
                        {
                            Level = node.Level + 1,
                            Settings = node.Settings,
                            Compilation = node.Compilation,
                            Filename = node.Filename + i,
                            BoundingBox = GetBoundingBox(node.BoundingBox, i)
                        };
                        
                        nodeQueue.Enqueue(n);
                        node.Children[i] = n;
                    }
                }

                previousNode = node;
            }
        }

        private Rect3D GetBoundingBox(Rect3D parentRect, int position)
        {
            Rect3D returnRect = new Rect3D()
            {
                X = parentRect.X,
                Y = parentRect.Y,
                Z = parentRect.Z,
                SizeX = parentRect.SizeX / 2,
                SizeY = parentRect.SizeY / 2,
                SizeZ = parentRect.SizeZ / 2
            };

            switch (position)
            {
                case 1:
                    returnRect.Y += returnRect.SizeY;
                    break;
                case 2:
                    returnRect.Z += returnRect.SizeZ;
                    break;
                case 3:
                    returnRect.Y += returnRect.SizeY;
                    returnRect.Z += returnRect.SizeZ;
                    break;
                case 4:
                    returnRect.X += returnRect.SizeX;
                    break;
                case 5:
                    returnRect.X += returnRect.SizeX;
                    returnRect.Y += returnRect.SizeY;
                    break;
                case 6:
                    returnRect.X += returnRect.SizeX;
                    returnRect.Z += returnRect.SizeZ;
                    break;
                case 7:
                    returnRect.X += returnRect.SizeX;
                    returnRect.Y += returnRect.SizeY;
                    returnRect.Z += returnRect.SizeZ;
                    break;
            }

            return returnRect;
        }

        public void UpdateCompilation(int level=0)
        {
            var node = this;
            
            while(node._next != null && Compilation.Points.Count < Settings.MaxPointNumber && node.Level <= level)
            {
                node.ReadPoints();
                node = node._next;
            }
        }

        private void ReadPoints()
        {
            using (BinaryReader b = new BinaryReader(File.Open(Settings.OctreeDir + @"\" + Filename + ".bin", FileMode.Open)))
            {
                int pos = 0;

                int length = (int)b.BaseStream.Length;
                while (pos < length)
                {
                    var p = new Points();

                    //POSITION_CARTESIAN
                    p.x = (float)(b.ReadInt32() * Settings.Scale + BoundingBox.X);
                    p.z = (float)(b.ReadInt32() * Settings.Scale + BoundingBox.Z);
                    p.y = (float)(b.ReadInt32() * Settings.Scale + BoundingBox.Y);
                    pos += 3 * sizeof(int);

                    //COLOR_PACKED
                    p.r = b.ReadByte();
                    p.g = b.ReadByte();
                    p.b = b.ReadByte();
                    p.a = b.ReadByte();
                    pos += 4 * sizeof(byte);

                    Compilation.Points.Add(p);
                }
            }
        }
    }
}
