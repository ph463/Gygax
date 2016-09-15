using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GygaxCore.Interfaces;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;


namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for PointcloudControl.xaml
    /// </summary>
    public partial class Control3D: INotifyPropertyChanged
    {
        private IStreamable _streamableObject;

        public static Camera GlobalCamera;

        private Camera LocalCamera;

        private static bool _useGlobalCamera = false;

        public bool UseGlobalCamera
        {
            get
            {
                return _useGlobalCamera;
            }
            set
            {
                if (_useGlobalCamera == value) return;

                _useGlobalCamera = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("UseGlobalCamera"));
            }
        }

        public IStreamable StreamableObject
        {
            get
            {
                return _streamableObject;
            }
            set
            {
                _streamableObject = value;
                if(_streamableObject != null)
                    _streamableObject.PropertyChanged += OnPropertyChanged;
            }
        }

        public Control3D()
        {
            InitializeComponent();

            Viewport.RenderTechniquesManager = new DefaultRenderTechniquesManager();
            Viewport.RenderTechnique = Viewport.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Blinn];
            Viewport.EffectsManager = new DefaultEffectsManager(Viewport.RenderTechniquesManager);

            InitViewport();


            SetBinding(DataContextProperty, new Binding());
            
            LocalCamera = Viewport.Camera;

            if (GlobalCamera == null)
            {
                GlobalCamera = Viewport.Camera;
            }

            //Viewport.Camera = GlobalCamera;

            PropertyChanged += OnPropertyChanged;

            IsVisibleChanged += OnIsVisibleChanged;
        }

        public void InitViewport()
        {
            Viewport.Items.Clear();

            var model = new AmbientLight3D {Color = new Color4(1, 1, 1, 1)};

            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model.Attach(Viewport.RenderHost);
            Viewport.Items.Add(model);

            var model2 = new DirectionalLight3D {Direction = new Vector3(1, 1, 1)};
            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model2.Attach(Viewport.RenderHost);
            Viewport.Items.Add(model2);

            var model3 = new DirectionalLight3D { Direction = new Vector3(-1, -1, -1) };
            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model3.Attach(Viewport.RenderHost);

            Viewport.Items.Add(model3);
            
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!IsVisible)
            {
                Viewport.Items.Clear();
                
            }
        }

        public void SetCamera(object sender, EventArgs e)
        {
            Viewport.Camera = GlobalCamera;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "UseGlobalCamera":

                    break;

                case "Data":
                case "DataContext":
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        //var vis = (Visualizer.Visualizer)_streamableObject.Data;

                        InitViewport();

                        var vis = Visualizer.Visualizer.GetModels(_streamableObject.Data);


                        foreach (var model in vis)
                        {
                            if (Viewport.RenderHost.RenderTechnique != null)
                                model.Attach(Viewport.RenderHost);

                            Viewport.Items.Add(model);
                        }

                        ViewportHelper.ZoomExtents(Viewport);

                        Viewport.Visibility = Visibility.Visible;
                        LoadingAnimation.Visibility = Visibility.Hidden;
                    });
                    break;
            }


        }

        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
            "DataContext",
            typeof(Object),
            typeof(Control3D),
            new PropertyMetadata(DataContextChanged)
        );

        public event PropertyChangedEventHandler PropertyChanged;

        private static void DataContextChanged(object sender,DependencyPropertyChangedEventArgs e)
        {
            Control3D myControl = (Control3D)sender;

            if (e.NewValue == null)
                return;
            
            myControl.StreamableObject = e.NewValue as IStreamable;

            if (myControl.StreamableObject.Data != null)
            {
                myControl.OnPropertyChanged(myControl.StreamableObject.Data,
                    new PropertyChangedEventArgs("DataContext"));
            }
        }

        private void Viewport_OnMouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void Viewport_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var hits = Viewport.FindHits(e.GetPosition(this));

            if (hits.Count == 0) return;

            var txt = hits[0].PointHit.X + "," + hits[0].PointHit.Y + "," + hits[0].PointHit.Z;

            Console.WriteLine(txt);
            Clipboard.SetText(txt);

            //ViewportHelper.AddSphere(hits[0].PointHit.ToVector3(), 0.1, PhongMaterials.Yellow, Viewport);

        }


    }
}
