using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GygaxCore;
using GygaxCore.Ifc;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Color = SharpDX.Color;

namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for Common3DSpace.xaml
    /// </summary>
    public partial class Common3DSpace : UserControl
    {
        public ObservableCollection<IStreamable> Items
        {
            get; set;
        }

        public PhongMaterial Material = PhongMaterials.Red;

        public Common3DSpace()
        {
            InitializeComponent();

            Viewport.RenderTechniquesManager = new DefaultRenderTechniquesManager();
            Viewport.RenderTechnique = Viewport.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Blinn];
            Viewport.EffectsManager = new DefaultEffectsManager(Viewport.RenderTechniquesManager);

            SetLight();

            SetBinding(DataContextProperty, new Binding());

            Viewport.MouseDoubleClick += ViewportOnMouseDoubleClick;
        }

        private void ViewportOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var x = Viewport.CurrentPosition;
        }

        private void SetLight()
        {
            Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(1,1,1) });
            Viewport.Items.Add(new DirectionalLight3D { Direction = new SharpDX.Vector3(-1, -1, -1) });
        }

        public void UpdateView()
        {
            if(Visibility != Visibility.Visible)
                return;

            foreach (var streamable in Items)
            {
                foreach (var model in Visualizer.Visualizer.GetModels(streamable.Data))
                {
                    if (Viewport.RenderHost.RenderTechnique != null)
                    {
                        model.Attach(Viewport.RenderHost);
                    }
                    
                    Viewport.Items.Add(model);
                }
            }

            Viewport.ZoomExtents();
        }

        public LineGeometryModel3D GetGrid()
        {
            var lineBilder = new LineBuilder();

            for (int x = -1000; x < 1000; x += 100)
            {
                lineBilder.AddLine(new Vector3(x, 0, -1000), new Vector3(x, 0, 1000));
            }

            for (int z = -1000; z < 1000; z += 100)
            {
                lineBilder.AddLine(new Vector3(-1000, 0, z), new Vector3(1000, 0, z));
            }
            
            LineGeometryModel3D m = new LineGeometryModel3D();
            m.Geometry = lineBilder.ToLineGeometry3D();
            m.Color = Color.Black;
            m.Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            m.Attach(Viewport.RenderHost);

            return m;
        }

        public void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateView();
        }

        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
            "DataContext",
            typeof(Object),
            typeof(Common3DSpace),
            new PropertyMetadata(DataContextChanged)
        );

        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Common3DSpace myControl = (Common3DSpace)sender;
            myControl.Items = (e.NewValue as ViewModel).Items;

            if (myControl.Items != null)
            {
                myControl.Items.CollectionChanged += myControl.ItemsOnCollectionChanged;
            }
        }

    }
}
