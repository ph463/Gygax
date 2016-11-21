using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GygaxCore.Interfaces;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for PointcloudControl.xaml
    /// </summary>
    public partial class Control3D : UserControl
    {
        private IStreamable _streamableObject;

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
            
            SizeChanged += OnSizeChanged;
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (((Control3D) sender).ActualWidth > 300)
            {
                Viewport.ShowCoordinateSystem = true;
                Viewport.ShowViewCube = true;
                Viewport.IsEnabled = true;
            }
            else
            {
                Viewport.ShowCoordinateSystem = false;
                Viewport.ShowViewCube = false;
                Viewport.IsEnabled = false;
            }
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
    }
}
