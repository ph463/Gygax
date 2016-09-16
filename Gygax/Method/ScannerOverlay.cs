namespace GygaxVisu.Method
{
    class ScannerOverlay
    {

        //public ObservableCollection<IStreamable> Items
        //{
        //    get; set;
        //}

        //public PhongMaterial Material = PhongMaterials.Red;

        //public Common3DSpace()
        //{
        //    InitializeComponent();

        //    Viewport.RenderTechniquesManager = new DefaultRenderTechniquesManager();
        //    Viewport.RenderTechnique = Viewport.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Blinn];
        //    Viewport.EffectsManager = new DefaultEffectsManager(Viewport.RenderTechniquesManager);

        //    SetLight();

        //    SetBinding(DataContextProperty, new Binding());

        //    Viewport.MouseDoubleClick += ViewportOnMouseDoubleClick;
        //}

        //private void ViewportOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        //{
        //    var x = Viewport.CurrentPosition;
        //}

        //private void SetLight()
        //{
        //    Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(1, 1, 1) });
        //    Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(-1, -1, -1) });
        //}

        //public void UpdateView()
        //{
        //    if (this.Visibility != Visibility.Visible)
        //        return;

        //    //Viewport.CameraChanged += ViewportOnCameraChanged;

        //    //var cs = new CoordinateSystem();
        //    //cs.Correspondences.Add(
        //    //    new CoordinateSystem.Correspondence
        //    //    {
        //    //        LocalCoordinateSystem = new Vector3(18.513800f, -16.186800f, 8.517870f),
        //    //        //ParentCoordinateSystem = new Vector3(67.52321f, 1.321693f, -9.646323f)
        //    //        ParentCoordinateSystem = new Vector3(67.50526f, 1.321693f, -9.870606f)
        //    //    });

        //    //cs.Correspondences.Add(
        //    //    new CoordinateSystem.Correspondence
        //    //    {
        //    //        LocalCoordinateSystem = new Vector3(-13.222400f, 3.524000f, 2.603240f),
        //    //        //ParentCoordinateSystem = new Vector3(80.46745f, 0.7104687f, -10.68228f)
        //    //        ParentCoordinateSystem = new Vector3(80.44949f, 0.7104687f, -10.90657f)
        //    //    });

        //    //cs.Correspondences.Add(
        //    //    new CoordinateSystem.Correspondence
        //    //    {
        //    //        LocalCoordinateSystem = new Vector3(26.357100f, 6.064450f, 6.593220f),
        //    //        //ParentCoordinateSystem = new Vector3(67.52321f, -7.703621f, -9.646323f)
        //    //        ParentCoordinateSystem = new Vector3(67.50526f, -7.703621f, -9.870606f)
        //    //    });

        //    //cs.Correspondences.Add(
        //    //    new CoordinateSystem.Correspondence
        //    //    {
        //    //        LocalCoordinateSystem = new Vector3(-10.352300f, -4.950700f, 4.092800f),
        //    //        ParentCoordinateSystem = new Vector3(68.58219f, 1.23447f, -9.711514f)
        //    //    });




        //    //cs.AddTestelements();
        //    //cs.CalculateHomography();

        //    //foreach (var correspondence in cs.Correspondences)
        //    //{
        //    //    MeshBuilder mb = new MeshBuilder();
        //    //    MeshGeometryModel3D model = new MeshGeometryModel3D();

        //    //    mb.AddSphere(correspondence.ParentCoordinateSystem, 0.3);
        //    //    model.Geometry = mb.ToMeshGeometry3D();

        //    //    model.Transform = new TranslateTransform3D(0, 0, 0);
        //    //    model.Material = PhongMaterials.Red;

        //    //    model.Attach(Viewport.RenderHost);
        //    //    Viewport.Items.Add(model);


        //    //    mb = new MeshBuilder();
        //    //    model = new MeshGeometryModel3D();

        //    //    mb.AddSphere(cs.ConvertToParentCoordinate(correspondence.LocalCoordinateSystem), 0.3);

        //    //    model.Geometry = mb.ToMeshGeometry3D();

        //    //    model.Transform = new TranslateTransform3D(0, 0, 0);
        //    //    model.Material = PhongMaterials.Green;

        //    //    model.Attach(Viewport.RenderHost);
        //    //    Viewport.Items.Add(model);
        //    //}

        //    //var recon = NViewMatchLoader.Open(new Uri(@"Z:\Data\Bridges\NineWells\DSLR\Wingwall 1\Part 1\Dense.nvm"), false);

        //    //foreach (var camera in recon.CameraPositions)
        //    //{
        //    //    AddCamera(camera, cs);
        //    //}



        //    //var p1a = new Vector3(67.50526f, 1.321693f, -9.870606f);
        //    //var p1b = new Vector2(1548, 954);
        //    ////FindScanner(cameraPositions[0], p1a, p1b);

        //    //var p2a = new Vector3(80.44949f, 0.7104687f, -10.90657f);
        //    //var p2b = new Vector2(1044, 1032);
        //    ////FindScanner(cameraPositions[0], p2a, p2b);

        //    //var p3a = new Vector3(67.50526f, -7.703621f, -9.870606f);
        //    //var p3b = new Vector2(1488, 1293);

        //    //var p1a = new Vector3(82.1771621704102f, 2.58097863197327f, -11.064395904541f);
        //    //var p1b = new Vector2(987, 978);
        //    ////FindScanner(cameraPositions[0], p1a, p1b);

        //    //AddSphere(p1a, PhongMaterials.Red);

        //    //var p2a = new Vector3(65.3799667358398f, 3.57053875923157f, -10.5716314315796f);
        //    //var p2b = new Vector2(1648, 852);
        //    ////FindScanner(cameraPositions[0], p2a, p2b);

        //    //AddSphere(p2a, PhongMaterials.Green);

        //    //var p3a = new Vector3(4.00973224639893f, -2.38271284103394f, -9.4886417388916f);
        //    //var p3b = new Vector2(2627, 1220);

        //    //AddSphere(p3a, PhongMaterials.Blue);

        //    //new CameraPosition
        //    //{
        //    //    Type = CameraPosition.CameraType.Spherical,
        //    //    CameraCenter = new Vector3(0f, 0f, 272.45124f),
        //    //    Orientation = Quaternion.RotationYawPitchRoll(new AngleSingle(00, AngleType.Degree).Radians, 0, 0),
        //    //    File = new Uri(@"C:\Users\Philipp\Desktop\Addenbrookes Bridge\Exports\New_Project_Scan_029\panorama.jpg"),
        //    //    Image = new Image<Bgr, Byte>(@"C:\Users\Philipp\Desktop\Addenbrookes Bridge\Exports\New_Project_Scan_029\panorama.jpg")
        //    //},

        //    var cameraPositions =
        //        SceneScannerPositionLoader.ReadScannerPositions(@"Z:\06. Data\Bridges\Philipp\Bridge 1\IFC\scanpositions.txt");

        //    foreach (var camPos in cameraPositions)
        //    {
        //        camPos.File = @"Z:\06. Data\Bridges\Philipp\Bridge 1\Panoramas\low\" + camPos.Name + ".jpg";
        //        camPos.Image = new Image<Bgr, Byte>(camPos.File);
        //        camPos.Width = camPos.Image.Width;
        //        camPos.Height = camPos.Image.Height;

        //        //AddCamera(camPos, cs);
        //        AddCamera(camPos, null);
        //    }

        //    var triangles = new List<Triangle>();

        //    foreach (var streamable in Items)
        //    {
        //        if (streamable is IfcViewerWrapper)
        //        {
        //            var p = IfcControl.GetItems((IfcViewerWrapper)streamable);

        //            foreach (var item in p)
        //            {
        //                if (item.RenderHost == null)
        //                {
        //                    item.Attach(Viewport.RenderHost);
        //                }

        //                item.mapper = new UvMapper((MeshGeometry3D)item.Geometry, IfcControl.GetItems((IfcViewerWrapper)streamable))
        //                {
        //                    TextureFilename = @"C:\Users\Philipp\Desktop\texture\" + getValidPathName(item.IfcName) + ".jpg",
        //                    TextureHeight = item.TextureHeight,
        //                    TextureWidth = item.TextureWidth
        //                };

        //                item.mapper.GenerateIndexes();

        //                triangles.AddRange(item.mapper.Triangles);
        //            }

        //            foreach (var triangle in triangles)
        //            {
        //                triangle.CalculateVisibility(cameraPositions, triangles);
        //            }

        //            foreach (var item in p)
        //            {
        //                item.mapper.GenerateSurfaceImageryFromCameraList(cameraPositions, item.mapper.Triangles);

        //                //item.Material = getMaterial(item.IfcName);

        //                item.Material = new PhongMaterial()
        //                {
        //                    AmbientColor = new SharpDX.Color4(.3f, .3f, .3f, 1f),
        //                    DiffuseColor = new SharpDX.Color4(.8f, .8f, .8f, 1f),

        //                    DiffuseMap = new BitmapImage(new Uri(item.mapper.TextureFilename))
        //                };

        //                Viewport.Items.Add(item);
        //            }
        //        }
        //        else if (streamable is Pointcloud)
        //        {
        //            PointGeometryModel3D model = new PointGeometryModel3D();

        //            // This one is important, otherwise it will be just black
        //            //model.Color = Color.White;

        //            model.Color = new Color(new SharpDX.Vector3(255, 255, 255), 0.8f);

        //            model.Geometry = (PointGeometry3D)streamable.Data;

        //            model.Transform = new TranslateTransform3D(0, 0, 0);

        //            if (Viewport.RenderHost.RenderTechnique != null)
        //                model.Attach(Viewport.RenderHost);

        //            //model.Effect = new

        //            Viewport.Items.Add(model);
        //        }
        //    }




        //    Viewport.SetView(
        //                    new Point3D(-12.9766976285485, 283.310137869653, 19.9333841996975),
        //                    new Vector3D(-2.66537552204365, -24.5147552553968, -37.5046797147629),
        //                    new Vector3D(-0.0248258580860065, 0.936672189248406, -0.34932633261595),
        //                    0
        //                );
        //}

        //private void ViewportOnCameraChanged(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    Debug.WriteLine(Viewport.Camera.Position + " " + Viewport.Camera.LookDirection + " " + Viewport.Camera.UpDirection);
        //}

        //private PhongMaterial getMaterial(string ifcName)
        //{
        //    var returnColor = new SharpDX.Color3(.8f, .8f, .8f);

        //    if (ifcName.Contains("pierCap"))
        //    {
        //        returnColor = new SharpDX.Color3(0, 0.8f, 0);
        //    }
        //    else if (ifcName.Contains("deck"))
        //    {
        //        returnColor = new SharpDX.Color3(0.8f, 0, 0);
        //    }
        //    else if (ifcName.Contains("Concrete-Round-Column"))
        //    {
        //        returnColor = new SharpDX.Color3(0.8f, 0.8f, 0);
        //    }
        //    else if (ifcName.Contains("Surface"))
        //    {
        //        returnColor = new SharpDX.Color3(0.87f, 0.72f, .53f);
        //    }
        //    else if (ifcName.Contains("Foundation"))
        //    {
        //        returnColor = new SharpDX.Color3(0.12f, 0.56f, 0.8f);
        //    }
        //    else if (ifcName.Contains("Floor"))
        //    {
        //        returnColor = new SharpDX.Color3(0.12f, 0.56f, 0.8f);
        //    }
        //    else if (ifcName.Contains("Basic Wall"))
        //    {
        //        returnColor = new SharpDX.Color3(0.64f, 0.11f, 1f);
        //    }
        //    else
        //    {
        //        returnColor = new SharpDX.Color3(0.8f, 0, 0);
        //    }


        //    return new PhongMaterial()
        //    {
        //        AmbientColor = new SharpDX.Color4(.3f, .3f, .3f, 1f),
        //        DiffuseColor = new SharpDX.Color4(returnColor, 1f),

        //        //DiffuseMap = new BitmapImage(new Uri(uvMapper.TextureFilename))
        //    };
        //}

        //private void AddSphere(Vector3 coordinates, PhongMaterial material = null, float radius = 0.3f)
        //{
        //    MeshBuilder mb = new MeshBuilder();
        //    MeshGeometryModel3D model = new MeshGeometryModel3D();

        //    mb.AddSphere(new SharpDX.Vector3((float)coordinates.X, (float)coordinates.Y, (float)coordinates.Z), radius);
        //    model.Geometry = mb.ToMeshGeometry3D();

        //    model.Transform = new TranslateTransform3D(0, 0, 0);

        //    model.Material = material ?? PhongMaterials.Red;

        //    model.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(model);
        //}

        //private Vector3 FindScanner(CameraPosition cam, Vector3 p1a, Vector2 p1b, Vector3 p2a, Vector2 p2b, Vector3 p3a, Vector2 p3b)
        //{
        //    var horizontal = FindScanner(cam, new Vector2(p1a.X, p1a.Z), p1b, new Vector2(p2a.X, p2a.Z), p2b, new Vector2(p3a.X, p3a.Z), p3b);
        //    var vertical = FindScanner(cam, new Vector2(p1a.Y, p1a.Z), p1b, new Vector2(p2a.Y, p2a.Z), p2b, new Vector2(p3a.Y, p3a.Z), p3b, true);

        //    return new Vector3(horizontal.X, vertical.X, horizontal.Y);
        //}

        //private Vector2 FindScanner(CameraPosition cam, Vector2 p1a, Vector2 p1b, Vector2 p2a, Vector2 p2b, Vector2 p3a, Vector2 p3b, bool vertical = false)
        //{
        //    var imageSize = new SharpDX.Size2(5046, 2134);

        //    var alpha = 0.0;
        //    var beta = 0.0;

        //    if (vertical)
        //    {
        //        alpha = ((p3b.Y - p2b.Y) / imageSize.Height) * Math.PI;
        //        beta = ((p1b.Y - p3b.Y) / imageSize.Height) * Math.PI;
        //    }
        //    else
        //    {
        //        alpha = ((p3b.X - p2b.X) / imageSize.Width) * 2 * Math.PI;
        //        beta = ((p1b.X - p3b.X) / imageSize.Width) * 2 * Math.PI;
        //    }

        //    var a = Math.Atan((p3a.X - p1a.X) / (p3a.Y - p1a.Y)) - Math.Atan((p2a.X - p1a.X) / (p2a.Y - p1a.Y));
        //    var b = Math.Atan((p1a.X - p2a.X) / (p1a.Y - p2a.Y)) - Math.Atan((p3a.X - p2a.X) / (p3a.Y - p2a.Y));
        //    var c = Math.Atan((p2a.X - p3a.X) / (p2a.Y - p3a.Y)) - Math.Atan((p1a.X - p3a.X) / (p1a.Y - p3a.Y));

        //    var k1 = 1 / (Math.Atan(a) - Math.Atan(alpha));
        //    var k2 = 1 / (Math.Atan(b) - Math.Atan(beta));
        //    var k3 = 1 / (Math.Atan(c) - Math.Atan(2 * Math.PI - alpha - beta));

        //    var x = (k1 * p1a.X + k2 * p2a.X + k3 * p3a.X) / (k1 + k2 + k3);
        //    var z = (k1 * p1a.Y + k2 * p2a.Y + k3 * p3a.Y) / (k1 + k2 + k3);

        //    return new Vector2((float)x, (float)z);




        //    /*
        //    var imageSize = new Size2(5046, 2134);

        //    var u = (p1b.X - imageSize.Width / 2f) / imageSize.Width;
        //    var v = (p1b.Y - imageSize.Height / 2f) / imageSize.Height;
        //    var alpha = -u * Math.PI;
        //    var beta = v * Math.PI;

        //    var rx = new Matrix3x3(1,0,0,0,(float)Math.Cos(beta), (float)-Math.Sin(beta),0, (float)Math.Sin(beta), (float)Math.Cos(beta));
        //    var ry = new Matrix3x3((float)Math.Cos(alpha), 0, (float)Math.Sin(alpha),0,1,0,-(float)Math.Sin(alpha),0, (float)Math.Cos(alpha));

        //    var rxq = Quaternion.RotationMatrix((Matrix)rx);
        //    var ryq = Quaternion.RotationMatrix((Matrix)ry);

        //    var dir = PlaneReconstructor.Rotate(rxq*ryq*cam.Orientation, Vector3.UnitZ);

        //    //var scanner

        //    var linemodel = new LineGeometryModel3D();
        //    var lb = new LineBuilder();
        //    lb.AddLine(cam.CameraCenter, cam.CameraCenter + dir * 100);
        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(0, 255, 0);
        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);


        //    ////var x = Math.Sqrt((1 - z*z)/(1 + a*a));
        //    ////var y = a*x;


        //    ////var ut = (0.75 + Math.Atan2(d.Z, d.X) / (2 * Math.PI)) % 1;
        //    ////var vt = 0.5 - Math.Asin(d.Y) / Math.PI;

        //    //var dir = new Vector3((float)w.X, (float)w.Y , (float)w.Z);
        //    //var r1 = new Ray(p1a, 10*dir);

        //    //var linemodel = new LineGeometryModel3D();
        //    //var lb = new LineBuilder();

        //    //lb.AddLine(p1a, p1a + dir * 10);

        //    //linemodel.Geometry = lb.ToLineGeometry3D();
        //    //linemodel.Color = new Color(0, 255, 0);

        //    //linemodel.Attach(Viewport.RenderHost);
        //    //Viewport.Items.Add(linemodel);
        //    */
        //}

        //private void AddCamera(CameraPosition camera, CoordinateSystem cs)
        //{
        //    switch (camera.Type)
        //    {
        //        case CameraPosition.CameraType.Planar:
        //            AddCameraPlanar(camera, cs);

        //            break;
        //        case CameraPosition.CameraType.Spherical:
        //            AddCameraSpherical(camera, 0.2);
        //            break;
        //    }
        //}

        //private void AddCameraPlanar(CameraPosition camera, CoordinateSystem cs)
        //{
        //    var model = new MeshGeometryModel3D();
        //    var pb = new PrimitiveBuilder();

        //    var length = (float)(camera.FocalLength / Math.Sqrt(Math.Pow(camera.Height, 2) + Math.Pow(camera.Width, 2)));
        //    //var length = (float)camera.FocalLength;

        //    var p1 = camera.CameraCenter +
        //                     length * (PlaneReconstructor.GetCornerPointToAxis(camera, camera.Orientation,
        //                         MyProcessor.Direction.TopLeft));

        //    var p2 = camera.CameraCenter +
        //                     length * (PlaneReconstructor.GetCornerPointToAxis(camera, camera.Orientation,
        //                         MyProcessor.Direction.TopRight));

        //    var p3 = camera.CameraCenter +
        //                     length * (PlaneReconstructor.GetCornerPointToAxis(camera, camera.Orientation,
        //                         MyProcessor.Direction.BottomRight));

        //    var p4 = camera.CameraCenter +
        //                     length * (PlaneReconstructor.GetCornerPointToAxis(camera, camera.Orientation,
        //                         MyProcessor.Direction.BottomLeft));

        //    var p1p = cs.ConvertToParentCoordinate(p1);
        //    var p2p = cs.ConvertToParentCoordinate(p2);
        //    var p3p = cs.ConvertToParentCoordinate(p3);
        //    var p4p = cs.ConvertToParentCoordinate(p4);

        //    model.Geometry = pb.GetRect(p1p, p2p, p3p, p4p);

        //    //model.Material = new PhongMaterial
        //    //{
        //    //    DiffuseMap = new BitmapImage(camera.File)
        //    //};

        //    model.Material = PhongMaterials.Yellow;

        //    model.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(model);

        //    var linemodel = new LineGeometryModel3D();
        //    var lb = new LineBuilder();

        //    var camCen = cs.ConvertToParentCoordinate(camera.CameraCenter);

        //    lb.AddLine(camCen, p1p);
        //    lb.AddLine(camCen, p2p);
        //    lb.AddLine(camCen, p3p);
        //    lb.AddLine(camCen, p4p);

        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(255, 255, 0);

        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);
        //}

        //public void AddCameraSpherical(CameraPosition camera, double radius = 1)
        //{
        //    MeshBuilder mb = new MeshBuilder();
        //    MeshGeometryModel3D model = new MeshGeometryModel3D();

        //    mb.AddSphere(camera.CameraCenter, radius);
        //    var geometry = mb.ToMeshGeometry3D();

        //    for (int i = 0; i < geometry.TextureCoordinates.Count; i++)
        //    {
        //        var v = geometry.TextureCoordinates[i];
        //        v.X = 1 - v.X;
        //        geometry.TextureCoordinates[i] = v;
        //    }

        //    model.Geometry = geometry;
        //    model.Geometry.Colors = new Color4Collection(geometry.TextureCoordinates.Select(x => x.ToColor4()));

        //    model.Transform = new TranslateTransform3D(0, 0, 0);

        //    //model.Material = new PhongMaterial
        //    //{
        //    //    DiffuseMap = new BitmapImage(new Uri(camera.File))
        //    //};

        //    model.Material = PhongMaterials.Blue;

        //    model.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(model);

        //    var linemodel = new LineGeometryModel3D();
        //    var lb = new LineBuilder();
        //    lb.AddLine(camera.CameraCenter, camera.CameraCenter + PlaneReconstructor.Rotate(camera.Orientation, new Vector3(1, 0, 0)));
        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(255, 0, 0);
        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);


        //    linemodel = new LineGeometryModel3D();
        //    lb = new LineBuilder();
        //    lb.AddLine(camera.CameraCenter, camera.CameraCenter + PlaneReconstructor.Rotate(camera.Orientation, new Vector3(0, 1, 0)));
        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(0, 255, 0);
        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);

        //    linemodel = new LineGeometryModel3D();
        //    lb = new LineBuilder();
        //    lb.AddLine(camera.CameraCenter, camera.CameraCenter + PlaneReconstructor.Rotate(camera.Orientation, new Vector3(0, 0, 1)));
        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(0, 0, 255);
        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);

        //    linemodel = new LineGeometryModel3D();
        //    lb = new LineBuilder();
        //    lb.AddLine(camera.CameraCenter, camera.CameraCenter + camera.Orientation.Axis);
        //    linemodel.Geometry = lb.ToLineGeometry3D();
        //    linemodel.Color = new Color(255, 255, 0);
        //    linemodel.Attach(Viewport.RenderHost);
        //    Viewport.Items.Add(linemodel);
        //}

        //private static string getValidPathName(string path)
        //{
        //    return Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
        //}

        ////public LineGeometryModel3D GetGrid()
        ////{
        ////    var lineBilder = new LineBuilder();

        ////    for (int x = -1000; x < 1000; x+=100)
        ////    {
        ////        lineBilder.AddLine(new Vector3(x, 0, -1000), new Vector3(x, 0, 1000));
        ////    }

        ////    for (int z = -1000; z < 1000; z+=100)
        ////    {
        ////        lineBilder.AddLine(new Vector3(-1000,0, z), new Vector3(1000, 0, z));
        ////    }


        ////    LineGeometryModel3D m = new LineGeometryModel3D();
        ////    m.Geometry = lineBilder.ToLineGeometry3D();
        ////    m.Color = Color.Black;
        ////    m.Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
        ////    m.Attach(Viewport.RenderHost);

        ////    return m;
        ////}

        //public void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        //{
        //    UpdateView();
        //}

        //public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
        //    "DataContext",
        //    typeof(Object),
        //    typeof(Common3DSpace),
        //    new PropertyMetadata(DataContextChanged)
        //);

        //private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    Common3DSpace myControl = (Common3DSpace)sender;
        //    myControl.Items = (e.NewValue as ViewModel).Items;

        //    if (myControl.Items != null)
        //    {
        //        myControl.Items.CollectionChanged += myControl.ItemsOnCollectionChanged;
        //    }
        //}
    }
}
