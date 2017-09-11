using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using HelixToolkit.Wpf.SharpDX;
using NLog;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;

namespace GygaxVisu.Visualizer
{
    public class PointcloudVisualizer : Visualizer
    {
        public static GeometryModel3D[] GetModels(Pointcloud pointcloud)
        {
            return GetModels((PointGeometry3D) pointcloud.Data);
        }

        public static GeometryModel3D[] GetModels(PointGeometry3D pointcloud)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            PointGeometryModel3D model = new PointGeometryModel3D();

            // This one is important, otherwise it will be just black
            model.Color = Color.White;

            model.Geometry = pointcloud;

            model.Name = "Points";

            models.Add(model);

            return models.ToArray();
        }
        
        public static TreeViewItem GetTreeItems(Pointcloud data, GeometryModel3D[] models)
        {
            var treeItem = new TreeViewItem()
            {
                Header = data.Name,
                DataContext = data
            };

            if (models == null) return null;

            foreach (var model in models)
            {
                var subItem = new TreeViewItem()
                {
                    Header = model.Name,
                    DataContext = model
                };

                var numberOfPointsItem = new TreeViewItem()
                {
                    Header = "Number of Points: " + ((PointGeometryModel3D)model).Geometry.Positions.Count
                };

                subItem.Items.Add(numberOfPointsItem);


                var hideItem = new TreeViewItem()
                {
                    Header = "hide",
                };

                hideItem.MouseUp += delegate (object sender, MouseButtonEventArgs args)
                {
                    if (model.Visibility == Visibility.Visible)
                    {
                        model.Visibility = Visibility.Hidden;
                        hideItem.Header = "show";
                    }
                    else
                    {
                        model.Visibility = Visibility.Visible;
                        hideItem.Header = "hide";
                    }

                };

                subItem.Items.Add(hideItem);

                treeItem.Items.Add(subItem);
            }

            return treeItem;
        }
    }
}
