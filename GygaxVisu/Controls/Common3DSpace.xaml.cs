using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GygaxCore;
using GygaxCore.Ifc;
using GygaxCore.Interfaces;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using SharpDX;
using Xceed.Wpf.Toolkit.Core.Converters;
using Binding = System.Windows.Data.Binding;
using Color = SharpDX.Color;
using ContextMenu = System.Windows.Controls.ContextMenu;
using UserControl = System.Windows.Controls.UserControl;

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

            InitViewport();

            SetBinding(DataContextProperty, new Binding());

            Viewport.MouseDoubleClick += ViewportOnMouseDoubleClick;

            DatastreamTree.SelectedItemChanged += DatastreamTreeOnSelectedItemChanged;
        }

        private void DatastreamTreeOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
        {
            try
            {
                if (routedPropertyChangedEventArgs.OldValue != null)
                ((MeshGeometryModel3D)((TreeViewItem)routedPropertyChangedEventArgs.OldValue).DataContext).Material = originalMaterialOfSelectedItem;
                
                var element =
                    (MeshGeometryModel3D) ((TreeViewItem) routedPropertyChangedEventArgs.NewValue).DataContext;

                element.IsSelected = true;
                originalMaterialOfSelectedItem = (PhongMaterial) element.Material;

                element.Material = PhongMaterials.Blue;
            }
            catch (Exception)
            {
            }
        }

        private void ViewportOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var x = Viewport.CurrentPosition;
        }
        public void InitViewport()
        {
            Viewport.Items.Clear();

            var model = new AmbientLight3D { Color = new Color4(1, 1, 1, 1) };

            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model.Attach(Viewport.RenderHost);
            Viewport.Items.Add(model);

            var model2 = new DirectionalLight3D { Direction = new Vector3(1, 1, 1) };
            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model2.Attach(Viewport.RenderHost);
            Viewport.Items.Add(model2);

            var model3 = new DirectionalLight3D { Direction = new Vector3(-1, -1, -1) };
            if (Viewport.RenderHost != null && Viewport.RenderHost.RenderTechnique != null)
                model3.Attach(Viewport.RenderHost);

            Viewport.Items.Add(model3);

        }

        public void UpdateView()
        {
            if (Visibility != Visibility.Visible)
                return;

            foreach (var streamable in Items)
            {
                if (streamable.Name == null || streamable.Name == "")
                    continue;

                var foundOne = false;

                //Check if datastream is already in the tree
                foreach (var item in DatastreamTree.Items)
                {
                    if ((TreeViewItem)item == null || ((TreeViewItem) item).DataContext == streamable)
                    {
                        foundOne = true;
                        break;
                    }
                }

                if (foundOne) continue;

                var elements = Visualizer.Visualizer.GetModels(streamable.Data);

                DatastreamTree.Items.Add(Visualizer.Visualizer.GetTreeItems(streamable, elements));

                if (elements == null) continue;

                foreach (var model in elements)
                {
                    if (Viewport.RenderHost.RenderTechnique != null)
                    {
                        model.Attach(Viewport.RenderHost);
                    }

                    Viewport.Items.Add(model);
                }
            }

            ViewportHelper.ZoomExtents(Viewport);
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

        public void ViewModelOnClearWorkspace(object sender, EventArgs eventArgs)
        {
            Viewport.Items.Clear();
            DatastreamTree.Items.Clear();
        }

        private PhongMaterial originalMaterialOfSelectedItem;
        
        private void Viewport_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            var hits = Viewport.FindHits(e.GetPosition(this));

            if (hits.Count == 0) return;

            var txt = hits[0].PointHit.X + "," + hits[0].PointHit.Y + "," + hits[0].PointHit.Z;
            LogManager.GetCurrentClassLogger().Info("Hit at " + txt);
            
            Console.WriteLine(txt);
            System.Windows.Clipboard.SetText(txt);

            try
            {
                var element =
                    hits.OrderBy(q => q.Distance).Select(r => r.ModelHit).First(s => s.Visibility == Visibility.Visible);
                
                var treeItem = getTreeItem(element, DatastreamTree.Items);
                treeItem.IsSelected = true;
                treeItem.IsExpanded = true;
                
                ContextMenu contextMenu = new ContextMenu();
                contextMenu.Items.Add(Visualizer.Visualizer.GetTreeItems(treeItem, element));
                
                contextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
            }
        }

        private TreeViewItem getTreeItem(object element, ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                if (item.DataContext.Equals(element))
                    return item;

                if (item.Items.Count <= 0) continue;

                var ti = getTreeItem(element, item.Items);
                if (ti != null)
                    return ti;
            }

            return null;
        }
    }
}
