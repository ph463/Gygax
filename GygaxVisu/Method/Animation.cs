namespace GygaxVisu
{
    public class Animation
    {
        //private bool _play;
        //public int RotationPos = 0;

        //// public bool Rotating = false;

        //private bool _fade = false;
        //private bool StepFinished = false;

        //private int step = 0;

        //private void Play(object source, ElapsedEventArgs e)
        //{
        //    if (!_play)
        //        return;

        //    if (RotationPos == 4)
        //    {
        //        RotationPos = 0;
        //        fade = 1;
        //        _fade = true;
        //    }

        //    if (StepFinished)
        //    {
        //        Thread.Sleep(2000);

        //        if (step == 0 || step == 1)
        //        {
        //            RotationPos = 0;
        //            fade = 1;
        //            _fade = true;
        //        }
        //        else
        //        {
        //            _fade = false;
        //        }

        //        step++;
        //        StepFinished = false;
        //        Debug.WriteLine(step);
        //    }

        //    if (!_fade)
        //    {
        //        Rotate();
        //    }
        //    else
        //    {
        //        switch (step)
        //        {
        //            case 0:
        //                FadeInBIM();
        //                break;
        //            case 1:
        //                FadePointcloud();
        //                break;
        //            case 2:
        //                ColorizeBIM();
        //                break;
        //            case 3:
        //                ShowSimpleTexture();
        //                break;
        //            case 4:
        //                ShowSingleScanner();
        //                break;
        //            case 5:
        //                ShowAllScanners();
        //                break;
        //        }
        //    }

        //}

        //private float fade = 1;

        //private void FadeInBIM()
        //{
        //    var time = 60;

        //    var timer = new Timer();
        //    timer.Interval = time;
        //    timer.Elapsed += Play;
        //    timer.AutoReset = false;
        //    timer.Enabled = true;

        //    if (fade < 0.0)
        //    {
        //        StepFinished = true;
        //        return;
        //    }

        //    fade -= 0.1f;

        //    Dispatcher.BeginInvoke((Action)delegate {

        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is MeshGeometryModel3D)
        //            {
        //                var i = (MeshGeometryModel3D)item;
        //                var m = (PhongMaterial)i.Material;

        //                //m.AmbientColor = new Color(new Vector3(m.AmbientColor.Red, m.AmbientColor.Green, m.AmbientColor.Blue), (1-fade));
        //                m.DiffuseColor = new Color(new Vector3(m.DiffuseColor.Red, m.DiffuseColor.Green, m.DiffuseColor.Blue), (1 - fade));
        //                Viewport.SetView(Viewport.Camera.Position, Viewport.Camera.LookDirection + new Vector3D(0.001f, 0.001f, 0.001f), Viewport.Camera.UpDirection, 10);
        //            }
        //        }
        //    });
        //}

        //private void ShowSimpleTexture()
        //{
        //    Dispatcher.BeginInvoke((Action)delegate
        //    {
        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is MyMeshGeometryModel3D)
        //            {
        //                var i = (MyMeshGeometryModel3D)item;

        //                i.Material = new PhongMaterial()
        //                {
        //                    AmbientColor = new Color4(.3f, .3f, .3f, 1f),
        //                    DiffuseColor = new Color4(.8f, .8f, .8f, 1f),

        //                    DiffuseMap =
        //                        new BitmapImage(
        //                            new Uri(@"C:\Users\Philipp\Desktop\texturePattern\" + getValidPathName(i.IfcName) +
        //                                    ".bmp"))
        //                };

        //                Viewport.SetView(Viewport.Camera.Position,
        //                    Viewport.Camera.LookDirection + new Vector3D(0.001f, 0.001f, 0.001f),
        //                    Viewport.Camera.UpDirection, 10);
        //            }
        //        }

        //        StepFinished = true;

        //        var time = 60;

        //        var timer = new Timer();
        //        timer.Interval = time;
        //        timer.Elapsed += Play;
        //        timer.AutoReset = false;
        //        timer.Enabled = true;
        //    });
        //}

        //private void ShowSingleScanner()
        //{
        //    Dispatcher.BeginInvoke((Action)delegate
        //    {
        //        var cameraPositions =
        //            SceneScannerPosition.ReadScannerPositions(@"Z:\06. Data\Bridges\Philipp\Bridge 1\IFC\scanpositions.txt");

        //        foreach (var camPos in cameraPositions)
        //        {
        //            camPos.File = @"Z:\06. Data\Bridges\Philipp\Bridge 1\Panoramas\low\" + camPos.Name + ".jpg";
        //            camPos.Image = new Image<Bgr, Byte>(camPos.File);
        //            camPos.Width = camPos.Image.Width;
        //            camPos.Height = camPos.Image.Height;

        //            AddCamera(camPos, null);

        //            break;
        //        }

        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is MyMeshGeometryModel3D)
        //            {
        //                var i = (MyMeshGeometryModel3D)item;

        //                i.Material = new PhongMaterial()
        //                {
        //                    AmbientColor = new Color4(.3f, .3f, .3f, 1f),
        //                    DiffuseColor = new Color4(.8f, .8f, .8f, 1f),

        //                    DiffuseMap =
        //                        new BitmapImage(
        //                            new Uri(@"C:\Users\Philipp\Desktop\textureSingleScanner\" +
        //                                    getValidPathName(i.IfcName) + ".bmp"))
        //                };

        //                Viewport.SetView(Viewport.Camera.Position,
        //                    Viewport.Camera.LookDirection + new Vector3D(0.001f, 0.001f, 0.001f),
        //                    Viewport.Camera.UpDirection, 10);
        //            }
        //        }

        //        StepFinished = true;

        //        var time = 60;

        //        var timer = new Timer();
        //        timer.Interval = time;
        //        timer.Elapsed += Play;
        //        timer.AutoReset = false;
        //        timer.Enabled = true;
        //    });
        //}

        //private void ShowAllScanners()
        //{
        //    Dispatcher.BeginInvoke((Action)delegate
        //    {
        //        var cameraPositions =
        //            SceneScannerPosition.ReadScannerPositions(@"Z:\06. Data\Bridges\Philipp\Bridge 1\IFC\scanpositions.txt");

        //        foreach (var camPos in cameraPositions)
        //        {
        //            camPos.File = @"Z:\06. Data\Bridges\Philipp\Bridge 1\Panoramas\low\" + camPos.Name + ".jpg";
        //            //camPos.Image = new Image<Bgr, Byte>(camPos.File);
        //            //camPos.Width = camPos.Image.Width;
        //            //camPos.Height = camPos.Image.Height;

        //            AddCamera(camPos, null);
        //        }

        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is MyMeshGeometryModel3D)
        //            {
        //                var i = (MyMeshGeometryModel3D)item;

        //                i.Material = new PhongMaterial()
        //                {
        //                    AmbientColor = new Color4(.3f, .3f, .3f, 1f),
        //                    DiffuseColor = new Color4(.8f, .8f, .8f, 1f),

        //                    DiffuseMap =
        //                        new BitmapImage(
        //                            new Uri(@"C:\Users\Philipp\Desktop\textureScanner\" + getValidPathName(i.IfcName) +
        //                                    ".bmp"))
        //                };

        //                Viewport.SetView(Viewport.Camera.Position,
        //                    Viewport.Camera.LookDirection + new Vector3D(0.001f, 0.001f, 0.001f),
        //                    Viewport.Camera.UpDirection, 10);
        //            }
        //        }

        //        StepFinished = true;

        //        var time = 60;

        //        var timer = new Timer();
        //        timer.Interval = time;
        //        timer.Elapsed += Play;
        //        timer.AutoReset = false;
        //        timer.Enabled = true;
        //    });
        //}

        //private void ColorizeBIM()
        //{
        //    var time = 60;

        //    var timer = new Timer();
        //    timer.Interval = time;
        //    timer.Elapsed += Play;
        //    timer.AutoReset = false;
        //    timer.Enabled = true;

        //    if (fade < 0.1)
        //    {
        //        StepFinished = true;
        //        return;
        //    }

        //    fade -= 0.1f;

        //    Dispatcher.BeginInvoke((Action)delegate {

        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is MyMeshGeometryModel3D)
        //            {
        //                var i = (MyMeshGeometryModel3D)item;
        //                var m = (PhongMaterial)i.Material;

        //                var mn = getMaterial(i.IfcName);

        //                m.DiffuseColor = new Color(
        //                    new Vector3(
        //                        (mn.DiffuseColor.Red - m.DiffuseColor.Red) * (1 - fade) + m.DiffuseColor.Red,
        //                        (mn.DiffuseColor.Green - m.DiffuseColor.Green) * (1 - fade) + m.DiffuseColor.Green,
        //                        (mn.DiffuseColor.Blue - m.DiffuseColor.Blue) * (1 - fade) + m.DiffuseColor.Blue
        //                        ),
        //                    255);
        //                Viewport.SetView(Viewport.Camera.Position, Viewport.Camera.LookDirection + new Vector3D(0.001f, 0.001f, 0.001f), Viewport.Camera.UpDirection, 10);
        //            }
        //        }
        //    });
        //}

        //private void FadePointcloud()
        //{
        //    var time = 60;

        //    var timer = new Timer();
        //    timer.Interval = time + 10;
        //    timer.Elapsed += Play;
        //    timer.AutoReset = false;
        //    timer.Enabled = true;

        //    Dispatcher.BeginInvoke((Action)delegate
        //    {
        //        bool foundPCL = false;

        //        foreach (var item in Viewport.Items)
        //        {
        //            if (item is PointGeometryModel3D)
        //            {
        //                foundPCL = true;

        //                var i = (PointGeometryModel3D)item;

        //                if (i.Color.A < 0.1)
        //                {
        //                    Viewport.Items.Remove(item);
        //                    StepFinished = true;
        //                    return;
        //                }

        //                fade -= 0.1f;

        //                i.Color = new Color(new Vector3(255, 255, 255), fade);
        //            }
        //        }

        //        if (!foundPCL)
        //        {
        //            StepFinished = true;
        //            return;
        //        }
        //    });
        //}


        //private void Rotate()
        //{
        //    var time = 4000;

        //    var timer = new Timer();
        //    timer.Interval = time + 100;
        //    timer.Elapsed += Play;
        //    timer.AutoReset = false;
        //    timer.Enabled = true;

        //    Dispatcher.BeginInvoke((Action)delegate {

        //        switch (RotationPos)
        //        {
        //            case 0:
        //            case 4:
        //                Viewport.SetView(
        //                    new Point3D(-12.9766976285485, 283.310137869653, 19.9333841996975),
        //                    new Vector3D(-2.66537552204365, -24.5147552553968, -37.5046797147629),
        //                    new Vector3D(-0.0248258580860065, 0.936672189248406, -0.34932633261595),
        //                    time
        //                );
        //                break;
        //            case 1:
        //                Viewport.SetView(
        //                    new Point3D(-6.67357697598915, 266.301378627907, -16.6965249591707),
        //                    new Vector3D(-0.633805757250975, 0.301760031260756, 2.08153477900182),
        //                    new Vector3D(0.102010426361666, 0.936672189248409, -0.33502101844829),
        //                    time
        //                );
        //                break;
        //            case 2:
        //                Viewport.SetView(
        //                    new Point3D(-5.12624836768848, 266.836737595096, -14.7317819055959),
        //                    new Vector3D(-2.18113436555164, -0.233598935927773, 0.116791725427049),
        //                    new Vector3D(0.113041273302073, 0.993571855676602, -0.00605294454203754),
        //                    time
        //                );
        //                break;
        //            case 3:
        //                Viewport.SetView(
        //                    new Point3D(-50.49056812785, 287.944280663552, -59.7728780558896),
        //                    new Vector3D(5.06893688859302, -34.7689902905831, 44.0448304066697),
        //                    new Vector3D(0.0492208501160769, 0.902585284349877, 0.42768810176218),
        //                    time
        //                );
        //                break;
        //        }
        //    });


        //    RotationPos++;

        //}

        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (_play)
        //    {
        //        _play = false;
        //    }
        //    else
        //    {
        //        _play = true;
        //        RotationPos = 0;
        //        Play(this, null);
        //    }


        //}
    }
}
