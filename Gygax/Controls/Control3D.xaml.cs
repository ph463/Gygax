using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;

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

            Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(1, 1, 1) });
            Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(-1, -1, -1) });

            SetBinding(DataContextProperty, new Binding());
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                //var vis = (Visualizer.Visualizer)_streamableObject.Data;

                var vis = Visualizer.Visualizer.GetModels(_streamableObject.Data);


                foreach (var model in vis)
                {
                    if (Viewport.RenderHost.RenderTechnique != null)
                        model.Attach(Viewport.RenderHost);

                    Viewport.Items.Add(model);
                }
                
                Viewport.ZoomExtents();

                Viewport.Visibility = Visibility.Visible;
                LoadingAnimation.Visibility = Visibility.Hidden;
            });
        }

        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
            "DataContext",
            typeof(Object),
            typeof(Control3D),
            new PropertyMetadata(DataContextChanged)
        );

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
