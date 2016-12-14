using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using SharpDX;
using SharpDX.Direct3D11;
using IImage = Emgu.CV.IImage;
using Point = System.Drawing.Point;
using System.Linq;
using System.Windows.Media.Imaging;
using GygaxCore.DataStructures;

namespace GygaxCore.Ifc
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// 

    /// <summary>
    /// Types of supported movements
    /// </summary>
    enum MOVE_TYPE
    {
        ROTATE,
        PAN,
        ZOOM,
        NONE,
    }

    /// <summary>
    /// IFCItem presents a single ifc item for drawing 
    /// </summary>
    public class IFCItem
    {
        public void CreateItem(IFCItem parent, long ifcID, string ifcType, string globalID, string name, string desc)
        {

            this.parent = parent;
            this.next = null;
            this.child = null;
            this.globalID = globalID;
            this.ifcID = ifcID;
            this.ifcType = ifcType;
            this.description = desc;
            this.name = name;

            if (parent != null)
            {
                if (parent.child == null)
                {
                    parent.child = this;
                }
                else
                {
                    IFCItem NextChild = parent;

                    while (true)
                    {
                        if (NextChild.next == null)
                        {
                            NextChild.next = this;
                            break;
                        }
                        else
                        {
                            NextChild = NextChild.next;
                        }

                    }

                }

            }
        }
        public long ifcID = 0;
        public string globalID;
        public string ifcType;
        public string name;
        public string description;
        public IFCItem parent = null;
        public IFCItem next = null;
        public IFCItem child = null;
        public long noVerticesForFaces;
        public long noPrimitivesForFaces;
        public float[] verticesForFaces;
        public long[] indicesForFaces;
        public long vertexOffsetForFaces;
        public long indexOffsetForFaces;
        public long noVerticesForWireFrame;
        public int noPrimitivesForWireFrame;
        public float[] verticesForWireFrame;
        public long[] indicesForWireFrame;
        public long[] indicesForWireFrameLineParts;
        public long vertexOffsetForWireFrame;
        public long indexOffsetForWireFrame;

        //public IFCTreeItem ifcTreeItem = null;


    }

    /// <summary>
    /// class responsible to encapsulate an implementation for ray picking for closest object 
    /// </summary>
    public class IFCPicker
    {
        private Device _device = null;
        private Vector3 _center = new Vector3();
        private float _size = 0;
        private IFCItem _rootIfcItem = null;

        private double _dist = 0;
        private IFCItem _ifcItemPicked = null;
        Vector3 _rayPos = new Vector3();
        Vector3 _rayDir = new Vector3();

        public IFCPicker(Device device, Vector3 center, float size, IFCItem rootItem)
        {
            _device = device;
            _center = center;
            _size = size;
            _rootIfcItem = rootItem;

        }

        //CustomVertex.PositionNormal Normalize(CustomVertex.PositionNormal vertex)
        //{
        //    CustomVertex.PositionNormal vertexNormalized = new CustomVertex.PositionNormal();
        //    vertexNormalized = vertex;
        //    vertexNormalized.X = (float)(vertex.X - _center.X) / _size;
        //    vertexNormalized.Y = (float)(vertex.Y - _center.Y) / _size;
        //    vertexNormalized.Z = (float)(vertex.Z - _center.Z) / _size;

        //    return vertexNormalized;
        //}

        private bool IntersectTri(long noPrimitives, long[] indicesForFaces, float[] verticesForFaces, out double minDist)
        {
            minDist = Double.MaxValue;

            // For each triangle in the item check if it interacts with the ray

            for (int i = 0; i < noPrimitives; i++)
            {
                // Get Triangle of the ifcitem

                Vector3 first, second, third;
                first.X = (verticesForFaces[6 * indicesForFaces[3 * i + 0] + 0] - _center.X) / _size;
                first.Y = (verticesForFaces[6 * indicesForFaces[3 * i + 0] + 1] - _center.Y) / _size;
                first.Z = (verticesForFaces[6 * indicesForFaces[3 * i + 0] + 2] - _center.Z) / _size;
                second.X = (verticesForFaces[6 * indicesForFaces[3 * i + 1] + 0] - _center.X) / _size;
                second.Y = (verticesForFaces[6 * indicesForFaces[3 * i + 1] + 1] - _center.Y) / _size;
                second.Z = (verticesForFaces[6 * indicesForFaces[3 * i + 1] + 2] - _center.Z) / _size;
                third.X = (verticesForFaces[6 * indicesForFaces[3 * i + 2] + 0] - _center.X) / _size;
                third.Y = (verticesForFaces[6 * indicesForFaces[3 * i + 2] + 1] - _center.Y) / _size;
                third.Z = (verticesForFaces[6 * indicesForFaces[3 * i + 2] + 2] - _center.Z) / _size;

                //IntersectInformation info = new IntersectInformation();
                //if (Geometry.IntersectTri(first, second, third, _rayPos, _rayDir, out info))
                //{
                //    if (info.Dist < minDist)
                //    {
                //        minDist = info.Dist;
                //    }
                //}
            }

            if (minDist != Double.MaxValue)
                return true;

            return false;
        }

        private void FindPickedIfcItem(IFCItem ifcItem)
        {
            while (ifcItem != null)
            {
                bool isVisible = false;

                if (isVisible == true)
                {
                    if (ifcItem.noPrimitivesForFaces != 0 && ifcItem.noVerticesForFaces != 0 && ifcItem.noPrimitivesForFaces != 0)
                    {
                        if (ifcItem.verticesForFaces != null)
                        {
                            double minDistForIFCItem = Double.MaxValue;

                            if (IntersectTri(ifcItem.noPrimitivesForFaces, ifcItem.indicesForFaces, ifcItem.verticesForFaces, out minDistForIFCItem))
                            {
                                if (_dist > minDistForIFCItem)
                                {
                                    _dist = minDistForIFCItem;
                                    _ifcItemPicked = ifcItem;
                                }
                            }

                        }//  if (ifcItem.verticesForFaces != null )             
                    }
                }

                FindPickedIfcItem(ifcItem.child);

                ifcItem = ifcItem.next;

            } //   if (ifcItem != null)

        }

        // ---------------------------------------------------------------------
        // Implement Picking procedure
        // Notes from here: http://www.toymaker.info/Games/html/picking.html

        public IFCItem PickObject(Point location)
        {
            //// ---------------------------------------------------------------------
            //// Transform mouse point to world space point

            //Vector3 near = new Vector3(location.X, location.Y, 0f);
            //Vector3 far = new Vector3(location.X, location.Y, 1f);

            //near.Unproject(_device.Viewport, _device.Transform.Projection, _device.Transform.View, _device.Transform.World);
            //far.Unproject(_device.Viewport, _device.Transform.Projection, _device.Transform.View, _device.Transform.World);

            //// ---------------------------------------------------------------------
            ////Transform GUI coordinates to space coordinates

            //_rayPos = near;

            //_rayDir = far;
            //_rayDir.Subtract(near);
            //_rayDir.Normalize();

            //// ---------------------------------------------------------------------
            //// clean data in use
            //_ifcItemPicked = null;

            //_dist = Double.MaxValue;

            //FindPickedIfcItem(_rootIfcItem);

            _ifcItemPicked = null;
            return _ifcItemPicked;
        }
    }

    /// <summary>
    /// Class aims to read ifc file and draw its objects 
    /// </summary>
    public class IfcViewerWrapper : IStreamable
    {
        private Device _device = null;
        public IFCItem RootIfcItem = null;
        private readonly Control _destControl = null;
        //private TreeView _treeControl = null;
        //public CIFCTreeData _treeData = new CIFCTreeData();
        private Vector3 _vEyePt = new Vector3(1.5f, 0, .5f);
        private Vector3 _vTargetPt = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 _vUpVector = new Vector3(0.0f, 0.0f, 1.0f);
        private int _counter = 0;
        private float _valueZ = 0;
        private float _valueX = 0;
        private MOVE_TYPE _currentMoveType = MOVE_TYPE.NONE;
        private Point _downPoint = new Point(-1, -1);
        private Point _startPoint = new Point(-1, -1);
        private Point _endPoint = new Point(-1, -1);
        private bool _enableWireFrames = true;
        private bool _enableFaces = true;
        private bool _enableHover = true;
        private int currentPos = 0;
        private int currentPosInd = 0;
        private float roll_val = 0.0f;
        private float pitch_val = 0.0f;
        private float yaw_val = 0.0f;
        private float _zoomIndex = 0F;
        private Vector3 _panVector = new Vector3(0, 0, 0);
        //private VertexBuffer m_vertexBuffer = null;
        //private IndexBuffer m_indexBuffer = null;
        Material _mtrlDefault;
        Material _mtrlBlack;
        Material _mtrlRed;

        private IFCItem _hoverIfcItem = null;
        private IFCItem _selectedIfcItem = null;
        Vector3 center = new Vector3();
        float size = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string Location { get; private set; }

        public event Streamable.ClosingEvent OnClosing;

        public string Name { get; private set; }

        public TreeNode<TreeElement> Tree { get; private set; }

        public object Data { get; set; }


        public IfcViewerWrapper(string sPath)
        {
            Location = sPath;
            Name = "Ifc File" + Path.GetFileName(sPath).Split('.').First();

            logger.Info("Loading ifc file " + Location);

            var thread = new Thread(LoadThreadFunction)
            {
                Name = "Ifc loader " + Path.GetFileName(Location)
            };
            thread.Start();
        }

        private void LoadThreadFunction()
        {
            if (true == File.Exists(Location))
            {
                //int ifcModel = IfcEngine.x86.sdaiOpenModelBN(0, sPath, "IFC2X3_TC1.exp");
                //Int64 ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, sPath,"IFC2X3_TC1.exp");

                var path = System.Text.Encoding.UTF8.GetBytes(Location);

                Int64 ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, Location, "Ifc/IFC2X3_TC1.exp");

                //Int64 ifcModel = IfcEngine.x64.sdaiCreateModelBNUnicode(0, System.Text.Encoding.UTF8.GetBytes("model.ifc"),
                //    System.Text.Encoding.UTF8.GetBytes("IFC2X3_TC1.exp"));

                string xmlSettings_IFC2x3 = @"IFC2X3-Settings.xml";
                string xmlSettings_IFC4 = @"IFC4-Settings.xml";

                if (ifcModel != 0)
                {

                    IntPtr outputValue = IntPtr.Zero;

                    IfcEngine.x64.GetSPFFHeaderItem(ifcModel, 9, 0, IfcEngine.x64.sdaiSTRING, out outputValue);

                    string s = Marshal.PtrToStringAnsi(outputValue);


                    XmlTextReader textReader = null;
                    if (s.Contains("IFC2") == true)
                    {
                        textReader = new XmlTextReader(xmlSettings_IFC2x3);
                    }
                    else
                    {
                        if (s.Contains("IFC4") == true)
                        {
                            IfcEngine.x64.sdaiCloseModel(ifcModel);
                            ifcModel = IfcEngine.x64.sdaiOpenModelBN(0, Location, "Ifc/IFC4.exp");

                            if (ifcModel != 0)
                                textReader = new XmlTextReader(xmlSettings_IFC4);
                        }
                    }

                    if (textReader == null)
                    {
                        logger.Info("Problem while loading ifc file");
                        return;
                    }

                    // if node type us an attribute
                    while (textReader.Read())
                    {
                        textReader.MoveToElement();

                        if (textReader.AttributeCount > 0)
                        {
                            if (textReader.LocalName == "object")
                            {
                                if (textReader.GetAttribute("name") != null)
                                {
                                    string Name = textReader.GetAttribute("name").ToString();
                                    //string Desc = textReader.GetAttribute("description").ToString();

                                    RetrieveObjects(ifcModel, Name, Name);
                                }
                            }
                        }
                    }

                    int a = 0;
                    GenerateGeometry(ifcModel, RootIfcItem, ref a);
                    
                    #region commented

                    /*                    // -----------------------------------------------------------------
                                        // Generate WireFrames Geometry

                                        int setting = 0, mask = 0;
                                        mask += IFCViewerModule.flagbit2;        //    PRECISION (32/64 bit)
                                        mask += IFCViewerModule.flagbit3;        //	   INDEX ARRAY (32/64 bit)
                                        mask += IFCViewerModule.flagbit5;        //    NORMALS
                                        mask += IFCViewerModule.flagbit8;        //    TRIANGLES
                                        mask += IFCViewerModule.flagbit12;       //    WIREFRAME
                                        setting += 0;		     //    DOUBLE PRECISION (double)

                                        if (IntPtr.Size == 4) // indication for 32
                                        {
                                            setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                                        }
                                        else
                                        {
                                            if (IntPtr.Size == 8)
                                            {
                                                setting += IFCViewerModule.flagbit3;     // 64 BIT INDEX ARRAY (Int64)
                                            }
                                        }

                                        setting += 0;            //    NORMALS OFF
                                        setting += 0;			 //    TRIANGLES OFF
                                        setting += IFCViewerModule.flagbit12;    //    WIREFRAME ON


                                        IFCViewerModule.setFormat(ifcModel, setting, mask);

                                        GenerateWireFrameGeometry(ifcModel, _rootIfcItem);
                                        // -----------------------------------------------------------------
                                        // Generate Faces Geometry

                                        setting = 0;
                                        setting += 0;		     //    SINGLE PRECISION (float)
                                        //#ifndef	WIN64
                                        if (IntPtr.Size == 4) // indication for 32
                                        {
                                            setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                                        }
                                        else
                                        {
                                            if (IntPtr.Size == 8)
                                            {
                                                setting += IFCViewerModule.flagbit3;     //    64 BIT INDEX ARRAY (Int64)
                                            }
                                        }

                                        setting += IFCViewerModule.flagbit5;     //    NORMALS ON
                                        setting += IFCViewerModule.flagbit8;     //    TRIANGLES ON
                                        setting += 0;			 //    WIREFRAME OFF 
                                        IFCViewerModule.setFormat(ifcModel, setting, mask);

                                        GenerateFacesGeometry(ifcModel, _rootIfcItem);
                    */

                    // -----------------------------------------------------------------
                    // Generate Tree Control
                    //_treeData.BuildTree(this, ifcModel, _rootIfcItem, _treeControl);

                    #endregion

                    var ifcTree = new IfcTree();
                    Tree = ifcTree.GetIfcTree(ifcModel);


                    // -----------------------------------------------------------------

                    IfcEngine.x64.sdaiCloseModel(ifcModel);

                    this.Data = this;

                    logger.Info("Ifc file loaded");
                    return;
                }
            }

            logger.Info("Problem while loading ifc file");
            return;
        }

        public static string GetValidPathName(string path)
        {
            return Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private void GenerateWireFrameGeometry(long ifcModel, IFCItem ifcItem)
        {
            if (ifcItem.ifcID != 0)
            {
                long noVertices = 0, noIndices = 0;
                IfcEngine.x64.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

                if (noVertices != 0 && noIndices != 0)
                {
                    ifcItem.noVerticesForWireFrame = noVertices;
                    ifcItem.verticesForWireFrame = new float[3 * noVertices];
                    ifcItem.indicesForWireFrame = new long[noIndices];

                    float[] pVertices = new float[noVertices * 3];

                    IfcEngine.x64.finalizeModelling(ifcModel, pVertices, ifcItem.indicesForWireFrame, 0);

                    int i = 0;
                    while (i < noVertices)
                    {
                        ifcItem.verticesForWireFrame[3 * i + 0] = pVertices[3 * i + 0];
                        ifcItem.verticesForWireFrame[3 * i + 1] = pVertices[3 * i + 1];
                        ifcItem.verticesForWireFrame[3 * i + 2] = pVertices[3 * i + 2];

                        i++;
                    };

                    ifcItem.noPrimitivesForWireFrame = 0;
                    ifcItem.indicesForWireFrameLineParts = new long[2 * noIndices];

                    long faceCnt = IfcEngine.x64.getConceptualFaceCnt(ifcItem.ifcID);

                    for (int j = 0; j < faceCnt; j++)
                    {
                        long startIndexFacesPolygons = 0, noIndicesFacesPolygons = 0;
                        long nonValue = 0;
                        long nonValue1 = 0;
                        long nonValue2 = 0;
                        IfcEngine.x64.getConceptualFaceEx(ifcItem.ifcID, j, ref nonValue, ref nonValue, ref nonValue, ref nonValue, ref nonValue, ref nonValue1, ref startIndexFacesPolygons, ref noIndicesFacesPolygons, ref nonValue2, ref nonValue2);

                        i = 0;
                        long lastItem = -1;
                        while (i < noIndicesFacesPolygons)
                        {
                            if (lastItem >= 0 && ifcItem.indicesForWireFrame[startIndexFacesPolygons + i] >= 0)
                            {
                                ifcItem.indicesForWireFrameLineParts[2 * ifcItem.noPrimitivesForWireFrame + 0] = lastItem;
                                ifcItem.indicesForWireFrameLineParts[2 * ifcItem.noPrimitivesForWireFrame + 1] = ifcItem.indicesForWireFrame[startIndexFacesPolygons + i];
                                ifcItem.noPrimitivesForWireFrame++;
                            }
                            lastItem = ifcItem.indicesForWireFrame[startIndexFacesPolygons + i];
                            i++;
                        }
                    }
                }
            }
        }

        private void GenerateFacesGeometry(long ifcModel, IFCItem ifcItem)
        {
            if (ifcItem.ifcID != 0)
            {
                long noVertices = 0, noIndices = 0;
                IfcEngine.x64.initializeModellingInstance(ifcModel, ref noVertices, ref noIndices, 0, ifcItem.ifcID);

                if (noVertices != 0 && noIndices != 0)
                {
                    ifcItem.noVerticesForFaces = noVertices;
                    ifcItem.noPrimitivesForFaces = noIndices / 3;
                    ifcItem.verticesForFaces = new float[6 * noVertices];
                    ifcItem.indicesForFaces = new long[noIndices];

                    float[] pVertices = new float[noVertices * 6];

                    IfcEngine.x64.finalizeModelling(ifcModel, pVertices, ifcItem.indicesForFaces, 0);

                    int i = 0;
                    while (i < noVertices)
                    {
                        ifcItem.verticesForFaces[6 * i + 0] = pVertices[6 * i + 0];
                        ifcItem.verticesForFaces[6 * i + 1] = pVertices[6 * i + 1];
                        ifcItem.verticesForFaces[6 * i + 2] = pVertices[6 * i + 2];

                        ifcItem.verticesForFaces[6 * i + 3] = pVertices[6 * i + 3];
                        ifcItem.verticesForFaces[6 * i + 4] = pVertices[6 * i + 4];
                        ifcItem.verticesForFaces[6 * i + 5] = pVertices[6 * i + 5];

                        i++;
                    }
                }
            }
        }

        void GenerateGeometry(long ifcModel, IFCItem ifcItem, ref int a)
        {
            while (ifcItem != null)
            {
                // -----------------------------------------------------------------
                // Generate WireFrames Geometry

                long setting = 0;
                long mask = 0;
                mask += IfcEngine.x64.flagbit2;        //    PRECISION (32/64 bit)
                mask += IfcEngine.x64.flagbit3;        //	   INDEX ARRAY (32/64 bit)
                mask += IfcEngine.x64.flagbit5;        //    NORMALS
                mask += IfcEngine.x64.flagbit8;        //    TRIANGLES
                mask += IfcEngine.x64.flagbit12;       //    WIREFRAME
                setting += 0;		     //    DOUBLE PRECISION (double)

                if (IntPtr.Size == 4) // indication for 32
                {
                    setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                }
                else
                {
                    if (IntPtr.Size == 8)
                    {
                        setting += IfcEngine.x64.flagbit3;     // 64 BIT INDEX ARRAY (Int64)
                    }
                }

                setting += 0;            //    NORMALS OFF
                setting += 0;			 //    TRIANGLES OFF
                setting += IfcEngine.x64.flagbit12;    //    WIREFRAME ON


                IfcEngine.x64.setFormat(ifcModel, setting, mask);

                GenerateWireFrameGeometry(ifcModel, ifcItem);
                // -----------------------------------------------------------------
                // Generate Faces Geometry

                setting = 0;
                setting += 0;		     //    SINGLE PRECISION (float)
                if (IntPtr.Size == 4) // indication for 32
                {
                    setting += 0;            //    32 BIT INDEX ARRAY (Int32)
                }
                else
                {
                    if (IntPtr.Size == 8)
                    {
                        setting += IfcEngine.x64.flagbit3;     //    64 BIT INDEX ARRAY (Int64)
                    }
                }

                setting += IfcEngine.x64.flagbit5;     //    NORMALS ON
                setting += IfcEngine.x64.flagbit8;     //    TRIANGLES ON
                setting += 0;			 //    WIREFRAME OFF 
                IfcEngine.x64.setFormat(ifcModel, setting, mask);

                GenerateFacesGeometry(ifcModel, ifcItem);

                IfcEngine.x64.cleanMemory(ifcModel, 0);

                GenerateGeometry(ifcModel, ifcItem.child, ref a);
                ifcItem = ifcItem.next;
            }
        }

        private void RetrieveObjects(long ifcModel, string sObjectSPFFName, string ObjectDisplayName)
        {
            long ifcObjectInstances = IfcEngine.x64.sdaiGetEntityExtentBN(ifcModel, ObjectDisplayName),
                noIfcObjectIntances = IfcEngine.x64.sdaiGetMemberCount(ifcObjectInstances);

            if (noIfcObjectIntances != 0)
            {
                IFCItem NewItem = null;
                if (RootIfcItem == null)
                {
                    RootIfcItem = new IFCItem();
                    RootIfcItem.CreateItem(null, 0, "", ObjectDisplayName, "", "");

                    NewItem = RootIfcItem;
                }
                else
                {
                    IFCItem LastItem = RootIfcItem;
                    while (LastItem != null)
                    {
                        if (LastItem.next == null)
                        {
                            LastItem.next = new IFCItem();
                            LastItem.next.CreateItem(null, 0, "", ObjectDisplayName, "", "");

                            NewItem = LastItem.next;

                            break;
                        }
                        else
                            LastItem = LastItem.next;
                    };
                }


                for (int i = 0; i < noIfcObjectIntances; ++i)
                {
                    long ifcObjectIns = 0;
                    IfcEngine.x64.engiGetAggrElement(ifcObjectInstances, i, IfcEngine.x64.sdaiINSTANCE, out ifcObjectIns);

                    IntPtr value = IntPtr.Zero;
                    IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "GlobalId", IfcEngine.x64.sdaiSTRING, out value);

                    string globalID = Marshal.PtrToStringAnsi((IntPtr)value);

                    value = IntPtr.Zero;
                    IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Name", IfcEngine.x64.sdaiSTRING, out value);

                    string name = Marshal.PtrToStringAnsi((IntPtr)value);

                    value = IntPtr.Zero;
                    IfcEngine.x64.sdaiGetAttrBN(ifcObjectIns, "Description", IfcEngine.x64.sdaiSTRING, out value);

                    string description = Marshal.PtrToStringAnsi((IntPtr)value);

                    IFCItem subItem = new IFCItem();
                    subItem.CreateItem(NewItem, ifcObjectIns, ObjectDisplayName, globalID, name, description);
                }
            }
        }

        /// <summary>
        /// Setup the lights and materials
        /// </summary>
        private void SetupLights()
        {
            //Light light = _device.Lights[0];
            //light.Type = LightType.Directional;
            //light.DiffuseColor = new ColorValue(3.4f, 3.4f, 3.4f, 3.4f);
            //light.SpecularColor = new ColorValue(0.1f, 0.1f, 0.1f, 0.5f);
            //light.AmbientColor = new ColorValue(0.5f, 0.5f, 0.5f, 1.0f);
            //light.Position = new Vector3(-2.0f, -2.0f, -2.0f);
            //light.Direction = Vector3.Normalize(new Vector3(-2.0f, -6.0f, -1.0f));
            //light.Range = 5.0f;
            //light.Enabled = true;

            //Light light1 = _device.Lights[1];
            //light1.Type = LightType.Directional;
            //light1.DiffuseColor = new ColorValue(3.4f, 3.4f, 3.4f, 3.4f);
            //light1.SpecularColor = new ColorValue(0.1f, 0.1f, 0.1f, 0.5f);
            //light1.AmbientColor = new ColorValue(0.5f, 0.5f, 0.5f, 1.0f);
            //light1.Position = new Vector3(2.0f, 2.0f, 2.0f);
            //light1.Direction = Vector3.Normalize(new Vector3(2.0f, 6.0f, 1.0f));
            //light1.Range = 5.0f;
            //light1.Enabled = true;

            //_device.RenderState.Lighting = true;

            //_device.SetRenderState(RenderStates.Lighting, true);

            //_device.SetRenderState(RenderStates.Ambient, 0x00707070);

            //_device.SetRenderState(RenderStates.CullMode, 0);
        }

        private void SetupMatrices()
        {
            //// -------------------------------------------------
            //// reset World Matrix
            //var World = Matrix.Identity;
            //World.M22 = -1f;

            //_device.Transform.World = World;

            //// -------------------------------------------------
            //// apply mouse rotation
            //if (roll_val != 0 || pitch_val != 0 || yaw_val != 0)
            //{
            //    Matrix rotationMatrix = Matrix.RotationYawPitchRoll(Geometry.DegreeToRadian(roll_val),
            //                                    Geometry.DegreeToRadian(pitch_val),
            //                                    Geometry.DegreeToRadian(yaw_val));


            //    _device.Transform.World = Matrix.Multiply(_device.Transform.World, rotationMatrix);


            //}

            //// -------------------------------------------------
            //// apply mouse zoom

            //if (_zoomIndex != 0)
            //{
            //    Matrix zoomMatrix = Matrix.Translation(new Vector3(_zoomIndex * vEyePt.X, _zoomIndex * vEyePt.Y, _zoomIndex * vEyePt.Z));

            //    _device.Transform.World = Matrix.Multiply(_device.Transform.World, zoomMatrix);

            //}

            //// -------------------------------------------------
            //// apply mouse pan by Z
            //if (valueZ != 0)
            //{

            //    _device.Transform.World = Matrix.Multiply(_device.Transform.World, Matrix.Translation(new Vector3(0, 0, valueZ)));

            //}

            //// -------------------------------------------------
            //// apply mouse pan by X
            //if (valueX != 0)
            //{

            //    _device.Transform.World = Matrix.Multiply(_device.Transform.World, Matrix.Translation(new Vector3(0, valueX, 0)));

            //}

            //// -------------------------------------------------
            //// default translation for better initial view

            //Matrix moveBack = Matrix.Translation(new Vector3(-0.5F, 0, 0));

            //_device.Transform.World = Matrix.Multiply(_device.Transform.World, moveBack);


            //// -------------------------------------------------
            //// setup Projection Matrix

            //_device.Transform.Projection =
            //Matrix.PerspectiveFovLH((float)Math.PI / 4.0F,
            //  (float)_destControl.Width / (float)_destControl.Height, 0.03f, 10.0f);

            //// -------------------------------------------------
            //// setup View Matrix

            //_device.Transform.View = Matrix.LookAtLH(vEyePt, vTargetPt, vUpVector);
        }

        private void GetDimensions(IFCItem ifcItem, ref Vector3 min, ref Vector3 max, ref bool InitMinMax)
        {
            while (ifcItem != null)
            {
                if (ifcItem.noVerticesForFaces != 0)
                {
                    if (InitMinMax == false)
                    {
                        min.X = ifcItem.verticesForFaces[3 * 0 + 0];
                        min.Y = ifcItem.verticesForFaces[3 * 0 + 1];
                        min.Z = ifcItem.verticesForFaces[3 * 0 + 2];
                        max = min;

                        InitMinMax = true;
                    }

                    int i = 0;
                    while (i < ifcItem.noVerticesForFaces)
                    {

                        min.X = Math.Min(min.X, ifcItem.verticesForFaces[6 * i + 0]);
                        min.Y = Math.Min(min.Y, ifcItem.verticesForFaces[6 * i + 1]);
                        min.Z = Math.Min(min.Z, ifcItem.verticesForFaces[6 * i + 2]);

                        max.X = Math.Max(max.X, ifcItem.verticesForFaces[6 * i + 0]);
                        max.Y = Math.Max(max.Y, ifcItem.verticesForFaces[6 * i + 1]);
                        max.Z = Math.Max(max.Z, ifcItem.verticesForFaces[6 * i + 2]);

                        i++;
                    }
                }

                GetDimensions(ifcItem.child, ref min, ref max, ref InitMinMax);

                ifcItem = ifcItem.next;
            }
        }

        private void InitalizeDeviceBuffer()
        {
            //Vector3 min = new Vector3();
            //Vector3 max = new Vector3();

            //bool InitMinMax = false;
            //GetDimensions(_rootIfcItem, ref min, ref max, ref InitMinMax);

            ////Vector3 center = new Vector3();
            //center = new Vector3();
            //center.X = (max.X + min.X) / 2f;
            //center.Y = (max.Y + min.Y) / 2f;
            //center.Z = (max.Z + min.Z) / 2f;

            ////float 
            //size = max.X - min.X;

            //if (size < max.Y - min.Y) size = max.Y - min.Y;
            //if (size < max.Z - min.Z) size = max.Z - min.Z;

            //int vBuffSize = 0, iBuffSize = 0;

            //GetBufferSizes_ifcFaces(_rootIfcItem, ref vBuffSize, ref iBuffSize);
            //GetBufferSizes_ifcWireFrame(_rootIfcItem, ref vBuffSize, ref iBuffSize);

            //if (vBuffSize == 0)
            //    return;

            //m_vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), vBuffSize, _device, Usage.None, VertexFormats.Normal, Pool.Managed);

            //CustomVertex.PositionNormal[] vertexArray = (CustomVertex.PositionNormal[])m_vertexBuffer.Lock(0, 0);

            //m_indexBuffer = new IndexBuffer(typeof(int), iBuffSize, _device, 0, Pool.Managed);

            //int[] indexArray = (int[])m_indexBuffer.Lock(0, 0);

            //currentPos = 0;
            //currentPosInd = 0;

            //FillBuffers_ifcFaces(_rootIfcItem, vertexArray, indexArray, center, size);
            //FillBuffers_ifcWireFrame(_rootIfcItem, vertexArray, indexArray, center, size);

            //Debug.Assert(currentPos == vBuffSize);
            //Debug.Assert(currentPosInd == iBuffSize);

            //m_indexBuffer.Unlock();
            //m_vertexBuffer.Unlock();
        }

        private void GetBufferSizes_ifcFaces(IFCItem item, ref long pVBuffSize, ref long pIBuffSize)
        {
            while (item != null)
            {
                if (item.ifcID != 0 && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
                {
                    item.vertexOffsetForFaces = pVBuffSize;
                    item.indexOffsetForFaces = pIBuffSize;

                    pVBuffSize += item.noVerticesForFaces;
                    pIBuffSize += 3 * item.noPrimitivesForFaces;
                }

                GetBufferSizes_ifcFaces(item.child, ref pVBuffSize, ref pIBuffSize);

                item = item.next;
            }
        }

        private void GetBufferSizes_ifcWireFrame(IFCItem item, ref long pVBuffSize, ref long pIBuffSize)
        {
            while (item != null)
            {
                if (item.ifcID != 0 && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
                {
                    item.vertexOffsetForWireFrame = pVBuffSize;
                    item.indexOffsetForWireFrame = pIBuffSize;

                    pVBuffSize += item.noVerticesForWireFrame;
                    pIBuffSize += 2 * item.noPrimitivesForWireFrame;
                }

                GetBufferSizes_ifcWireFrame(item.child, ref pVBuffSize, ref pIBuffSize);

                item = item.next;
            }
        }

        //private void FillBuffers_ifcWireFrame(IFCItem item, CustomVertex.PositionNormal[] vertexArray, int[] indexArray, Vector3 center, float size)
        //{

        //    while (item != null)
        //    {
        //        if (item.ifcID != 0 && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
        //        {
        //            if (item.verticesForWireFrame != null)
        //            {
        //                for (int i = 0; i < item.noVerticesForWireFrame; i++)
        //                {
        //                    vertexArray[currentPos + i].X = (item.verticesForWireFrame[3 * i + 0] - center.X) / size;
        //                    vertexArray[currentPos + i].Y = (item.verticesForWireFrame[3 * i + 1] - center.Y) / size;
        //                    vertexArray[currentPos + i].Z = (item.verticesForWireFrame[3 * i + 2] - center.Z) / size;

        //                    vertexArray[currentPos + i].Nx = 0f;
        //                    vertexArray[currentPos + i].Ny = 0f;
        //                    vertexArray[currentPos + i].Nz = 1f;
        //                }

        //                Debug.Assert(item.verticesForWireFrame.Length == item.noVerticesForWireFrame * 3);
        //            }

        //            if (item.indicesForWireFrameLineParts != null)
        //            {
        //                for (int i = 0; i < item.noPrimitivesForWireFrame; i++)
        //                {
        //                    indexArray[currentPosInd + 2 * i + 0] = item.indicesForWireFrameLineParts[2 * i + 0] + currentPos;
        //                    indexArray[currentPosInd + 2 * i + 1] = item.indicesForWireFrameLineParts[2 * i + 1] + currentPos;
        //                }

        //                //Debug.Assert(item.indicesForWireFrame.Length == 2 * item.noPrimitivesForWireFrame);
        //            }

        //            Debug.Assert(item.vertexOffsetForWireFrame == currentPos);
        //            Debug.Assert(item.indexOffsetForWireFrame == currentPosInd);

        //            currentPos += item.noVerticesForWireFrame;
        //            currentPosInd += 2 * item.noPrimitivesForWireFrame;
        //        }

        //        FillBuffers_ifcWireFrame(item.child, vertexArray, indexArray, center, size);
        //        item = item.next;
        //    }
        //}

        //private void FillBuffers_ifcFaces(IFCItem item, CustomVertex.PositionNormal[] vertexArray, int[] indexArray, Vector3 center, float size)
        //{
        //    while (item != null)
        //    {
        //        if (item.ifcID != 0 && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
        //        {
        //            if (item.verticesForFaces != null)
        //            {
        //                for (int i = 0; i < item.noVerticesForFaces; i++)
        //                {
        //                    vertexArray[currentPos + i].X = (item.verticesForFaces[6 * i + 0] - center.X) / size;
        //                    vertexArray[currentPos + i].Y = (item.verticesForFaces[6 * i + 1] - center.Y) / size;
        //                    vertexArray[currentPos + i].Z = (item.verticesForFaces[6 * i + 2] - center.Z) / size;
        //                    vertexArray[currentPos + i].Nx = item.verticesForFaces[6 * i + 3];
        //                    vertexArray[currentPos + i].Ny = item.verticesForFaces[6 * i + 4];
        //                    vertexArray[currentPos + i].Nz = item.verticesForFaces[6 * i + 5];
        //                }

        //                Debug.Assert(item.verticesForFaces.Length == item.noVerticesForFaces * 6);
        //            }

        //            if (item.indicesForFaces != null)
        //            {
        //                for (int i = 0; i < 3 * item.noPrimitivesForFaces; i++)
        //                {
        //                    indexArray[currentPosInd + i] = item.indicesForFaces[i] + currentPos;
        //                }
        //            }

        //            Debug.Assert(currentPos == item.vertexOffsetForFaces);
        //            Debug.Assert(currentPosInd == item.indexOffsetForFaces);
        //            currentPos += item.noVerticesForFaces;
        //            currentPosInd += 3 * item.noPrimitivesForFaces;
        //        }

        //        FillBuffers_ifcFaces(item.child, vertexArray, indexArray, center, size);
        //        item = item.next;
        //    }
        //}

        //private void Render()
        //{
        //    // -------------------------------------------------
        //    // setup rendering procedure
        //    _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
        //    _device.BeginScene();
        //    _device.VertexFormat = VertexFormats.Normal;

        //    SetupLights();

        //    SetupMatrices();

        //    _device.SetStreamSource(0, m_vertexBuffer, 0);
        //    _device.Indices = m_indexBuffer;

        //    // -------------------------------------------------
        //    // Render both faces and wireframe, starting from the root item

        //    if (Faces == true || _hoverIfcItem != null || _selectedIfcItem != null)
        //    {
        //        _device.Material = _mtrlDefault;
        //        RenderFaces(_rootIfcItem);
        //    }


        //    if (WireFrames)
        //    {
        //        _device.Material = _mtrlBlack;
        //        RenderWireFrame(_rootIfcItem);
        //    }

        //    // -------------------------------------------------
        //    _device.EndScene();
        //    _device.Present();
        //}

        //private void RenderWireFrame(IFCItem ifcItem)
        //{
        //    while (ifcItem != null)
        //    {
        //        if (ifcItem.noPrimitivesForWireFrame != 0)
        //        {
        //            if (ifcItem.ifcTreeItem.IsVisible)
        //                _device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, ifcItem.vertexOffsetForWireFrame, ifcItem.noVerticesForWireFrame, ifcItem.indexOffsetForWireFrame, ifcItem.noPrimitivesForWireFrame);
        //        }

        //        //NOTE: Prevouis approach was with just  RenderWireFrame(ifcItem.next); 
        //        // Due to the limit of stack memory size this always throws StackOverflowException if complex ifc file loaded
        //        RenderWireFrame(ifcItem.child);

        //        ifcItem = ifcItem.next;
        //    }
        //}

        //private void RenderFaces(IFCItem ifcItem)
        //{
        //    while (ifcItem != null)
        //    {
        //        if (ifcItem.noPrimitivesForFaces != 0)
        //        {
        //            System.Diagnostics.Debug.Assert(ifcItem.ifcTreeItem != null, "Internal error.");

        //            bool bRender = false;
        //            if ((ifcItem == _hoverIfcItem) || ifcItem == _selectedIfcItem)
        //            {
        //                // Even in Non-faces mode it is good to show selected faces
        //                _device.Material = _mtrlRed;
        //                bRender = true;
        //            }
        //            else
        //            {
        //                // If faces are enabled then 
        //                if (Faces)
        //                {
        //                    if (ifcItem.ifcTreeItem.ifcColor != null)
        //                    {
        //                        Material material = new Material();
        //                        material.DiffuseColor = material.AmbientColor = material.SpecularColor =
        //                                    new ColorValue(ifcItem.ifcTreeItem.ifcColor.R,
        //                                        ifcItem.ifcTreeItem.ifcColor.G,
        //                                        ifcItem.ifcTreeItem.ifcColor.B,
        //                                        1);
        //                        material.EmissiveColor =
        //                                    new ColorValue(ifcItem.ifcTreeItem.ifcColor.R / 2,
        //                                        ifcItem.ifcTreeItem.ifcColor.G / 2,
        //                                        ifcItem.ifcTreeItem.ifcColor.B / 2,
        //                                        0.5f);
        //                        material.SpecularSharpness = 0.5f;


        //                        _device.Material = material;
        //                    }
        //                    else
        //                    {
        //                        _device.Material = _mtrlDefault;
        //                    }

        //                    bRender = true;
        //                }
        //            }

        //            if (bRender && ifcItem.ifcTreeItem.IsVisible)
        //                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, ifcItem.vertexOffsetForFaces, ifcItem.noVerticesForFaces, ifcItem.indexOffsetForFaces, ifcItem.noPrimitivesForFaces);
        //        }
        //        //NOTE: Prevouis approach was with just  RenderFaces(ifcItem.next); 
        //        // Due to the limit of stack memory size this always throws StackOverflowException if complex ifc file loaded
        //        RenderFaces(ifcItem.child);

        //        ifcItem = ifcItem.next;
        //    } // while
        //}

        // -------------------------------------------------------------------
        // Public Methods 

        //public void OnMouseMove(MouseEventArgs e)
        //{
        //    if (counter == 0)
        //    {
        //        _startPoint = _endPoint;
        //        counter = 10;
        //    }
        //    else
        //        counter--;

        //    _endPoint = e.Location;
        //    float _stepTranslate = 0.01F;

        //    // check direction of movement
        //    bool MoveRight = (_startPoint.X <= _endPoint.X);

        //    // check direction of movement
        //    bool MoveUp = (_startPoint.Y >= _endPoint.Y);
        //    int deltaY = Math.Abs(_startPoint.Y - _endPoint.Y);
        //    int deltaX = Math.Abs(_startPoint.X - _endPoint.X);


        //    switch (_currentMoveType)
        //    {
        //        case MOVE_TYPE.ROTATE:
        //            {
        //                float stepRotate = 1.5F;

        //                if (deltaY >= deltaX)
        //                {
        //                    pitch_val += (MoveUp) ? stepRotate : -stepRotate;
        //                }
        //                else
        //                {
        //                    yaw_val += (MoveRight) ? -stepRotate : stepRotate;
        //                }

        //                break;
        //            };
        //        case MOVE_TYPE.PAN:
        //            {
        //                _stepTranslate = 0.005F;
        //                if (deltaY >= deltaX)
        //                {
        //                    valueZ += (MoveUp) ? _stepTranslate : -_stepTranslate;
        //                }
        //                else
        //                {
        //                    valueX += (MoveRight) ? -_stepTranslate : _stepTranslate;

        //                }


        //            }
        //            break;
        //        case MOVE_TYPE.ZOOM:
        //            {
        //                _zoomIndex += (MoveUp) ? -_stepTranslate : _stepTranslate;

        //                break;
        //            };
        //    }



        //    if (_currentMoveType != MOVE_TYPE.NONE)
        //    {
        //        this.Redraw();
        //    }
        //    else
        //    {
        //        if (_enableHover)
        //        {
        //            IFCPicker picker = null;
        //            picker = new IFCPicker(_device, center, size, _rootIfcItem);

        //            IFCItem newPickedItem = picker.PickObject(e.Location);

        //            if (_hoverIfcItem != null && newPickedItem == null)
        //            {
        //                _hoverIfcItem = null;

        //                this.Redraw();
        //            }
        //            else
        //            {
        //                if (_hoverIfcItem != newPickedItem)
        //                {
        //                    _hoverIfcItem = newPickedItem;

        //                    this.Redraw();
        //                }
        //            }
        //        }


        //    }

        //}
        public void OnMouseUp(MouseEventArgs e)
        {
            _currentMoveType = MOVE_TYPE.NONE;
        }



        //public void OnMouseDown(MouseEventArgs e)
        //{
        //    _currentMoveType = MOVE_TYPE.NONE;
        //    _startPoint = e.Location;
        //    _endPoint = _startPoint;


        //    switch (e.Button)
        //    {
        //        case MouseButtons.Left: _currentMoveType = MOVE_TYPE.ROTATE; break;
        //        case MouseButtons.Right: _currentMoveType = MOVE_TYPE.PAN; break;
        //        case MouseButtons.Middle: _currentMoveType = MOVE_TYPE.ZOOM; break;

        //    }

        //    if (e.Button == MouseButtons.Left)
        //    {
        //        IFCPicker picker = null;
        //        picker = new IFCPicker(_device, center, size, _rootIfcItem);

        //        IFCItem newPickedItem = picker.PickObject(e.Location);

        //        if (_selectedIfcItem != null && newPickedItem == null)
        //        {
        //            _selectedIfcItem = null;

        //            this.Redraw();
        //        }
        //        else
        //        {
        //            if (_selectedIfcItem != newPickedItem)
        //            {
        //                _selectedIfcItem = newPickedItem;

        //                // expand and select the corresponding tree item for this IFCItem
        //                _treeData.OnSelectIFCElement(_selectedIfcItem);

        //                this.Redraw();
        //            }
        //        }
        //    }

        //}

        public void Reset()
        {
            roll_val = 0.0f;
            pitch_val = 0.0f;
            yaw_val = 45.0f;
            _zoomIndex = 0F;
            _currentMoveType = MOVE_TYPE.NONE;
            _panVector = new Vector3(0, 0, 0);
            _valueZ = 0;
            _valueX = 0;
        }
        public bool WireFrames
        {
            get
            {
                return _enableWireFrames;
            }
            set
            {
                _enableWireFrames = value;
            }
        }
        public bool Faces
        {
            get
            {
                return _enableFaces;
            }
            set
            {
                _enableFaces = value;
            }
        }

        //public bool Hover
        //{
        //    get
        //    {
        //        return _enableHover;
        //    }
        //    set
        //    {
        //        _enableHover = value;

        //        if (_enableHover == false)
        //        {
        //            if (_hoverIfcItem != null)
        //            {
        //                _hoverIfcItem = null;

        //                Redraw();
        //            }
        //        }
        //    }
        //}

        //public void InitGraphics(Control destControl, TreeView destTreeControl)
        //{

        //    //TODO: adaptive model must be implemented to support different type of buffers
        //    // See Pick_2005 MS example

        //    _destControl = destControl;
        //    _treeControl = destTreeControl;
        //    PresentParameters present_params = new PresentParameters();

        //    //TODO: Choose the rignt multisampling accordingly
        //    present_params.MultiSample = MultiSampleType.None;
        //    present_params.SwapEffect = SwapEffect.Discard;
        //    present_params.EnableAutoDepthStencil = true;
        //    present_params.AutoDepthStencilFormat = DepthFormat.D16;
        //    //TODO: d3dpp.BackBufferFormat = d3ddm.Format;

        //    //TODO: CheckDeviceMultiSampleType

        //    present_params.Windowed = true;
        //    present_params.SwapEffect = SwapEffect.Discard;
        //    _device = new Device(0, DeviceType.Hardware, _destControl, CreateFlags.HardwareVertexProcessing, present_params);

        //    if (_device == null)
        //    {
        //        _device = new Device(0, DeviceType.Hardware, _destControl, CreateFlags.SoftwareVertexProcessing, present_params);
        //    }


        //    _device.SetRenderState(RenderStates.CullMode, 1);


        //}
        //public bool OpenIFCFile(string ifcFilePath)
        //{
        //    Reset();

        //    RootIfcItem = null;

        //    if (ParseIfcFile(ifcFilePath) == true)
        //    {
        //        InitalizeDeviceBuffer();

        //        this._destControl.Refresh();

        //        return true;
        //    }

        //    return false;


        //}
        //public void Redraw()
        //{
        //    this.Render();
        //}

        //public void SelectItem(IFCItem ifcItem)
        //{
        //    _selectedIfcItem = ifcItem;

        //    this.Redraw();
        //}
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource ImageSource { get; }
        public IImage CvSource { get; set; }
        public void Close()
        {
            OnClosing?.Invoke(this);
        }

        public void Save()
        {
        }

        public void Save(string filename)
        {

        }
    }
}
