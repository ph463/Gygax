using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using GygaxCore.Ifc;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;

namespace GygaxVisu.Visualizer
{
    public abstract class Visualizer
    {
        public static GeometryModel3D[] GetModels(object data)
        {
            if (data == null)
                return null;

            var t = data.GetType();

            if (t == typeof(NViewMatch))
            {
                return NViewMatchVisualizer.GetModels((NViewMatch)data);
            }

            if (t == typeof(NViewMatch[]))
            {
                return NViewMatchVisualizer.GetModels((NViewMatch[])data);
            }

            if (t == typeof(List<CameraPosition>))
            {
                return NViewMatchVisualizer.GetModels(new NViewMatch()
                {
                    CameraPositions = (List<CameraPosition>)data
                });
            }

            if (t == typeof(IfcViewerWrapper))
            {
                return IfcVisualizer.GetModels((IfcViewerWrapper)data);
            }

            if (t == typeof(Pointcloud))
            {
                return PointcloudVisualizer.GetModels((Pointcloud)data);
            }

            if (t == typeof(PointGeometry3D))
            {
                return PointcloudVisualizer.GetModels((PointGeometry3D)data);
            }

            if (t == typeof(CoordinateSystem))
            {
                return CoordinateSystemVisualizer.GetModels((CoordinateSystem)data);
            }

            return null;
        }

        public static TreeViewItem GetTreeItems(TreeViewItem treeNode, GeometryModel3D model)
        {
            var items = GetTreeItems((IStreamable)((TreeViewItem) (treeNode.Parent)).DataContext, new[] {model});

            var childItem = (TreeViewItem) items.Items[0];

            items.Items.Remove(childItem);
            
            return childItem;
        }

        public static TreeViewItem GetTreeItems(IStreamable data, GeometryModel3D[] models)
        {
            if (data == null)
                return null;

            var t = data.GetType();
            

            if (t == typeof(IfcViewerWrapper))
            {
                return IfcVisualizer.GetTreeItems((IfcViewerWrapper)data, models);
            }
            else
            {
                return GetStandardTreeItems(data, models);
            }
        }

        private static TreeViewItem GetStandardTreeItems(IStreamable data, GeometryModel3D[] models)
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

        protected static GeometryModel3D[] GetCamera(CameraPosition camera)
        {
            switch (camera.Type)
            {
                default:
                case CameraPosition.CameraType.Planar:
                    return GetCameraPlanar(camera);
                case CameraPosition.CameraType.Spherical:
                    return GetCameraSpherical(camera, 0.2);
            }
        }

        private static GeometryModel3D[] GetCameraPlanar(CameraPosition camera)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            var model = new MeshGeometryModel3D();
            var pb = new PrimitiveBuilder();

            var length = (camera.FocalLength / Math.Sqrt(Math.Pow(camera.Height, 2) + Math.Pow(camera.Width, 2)));

            var p1 = camera.CameraCenter +
                             length * (CameraPosition.GetCornerPointToAxis(camera, camera.Orientation,
                                 CameraPosition.Direction.TopLeft));

            var p2 = camera.CameraCenter +
                             length * (CameraPosition.GetCornerPointToAxis(camera, camera.Orientation,
                                 CameraPosition.Direction.TopRight));

            var p3 = camera.CameraCenter +
                             length * (CameraPosition.GetCornerPointToAxis(camera, camera.Orientation,
                                 CameraPosition.Direction.BottomRight));

            var p4 = camera.CameraCenter +
                             length * (CameraPosition.GetCornerPointToAxis(camera, camera.Orientation,
                                 CameraPosition.Direction.BottomLeft));

            model.Geometry = pb.GetRect(p1.ToVector3(), p2.ToVector3(), p3.ToVector3(), p4.ToVector3());

            //model.Material = new PhongMaterial
            //{
            //    DiffuseMap = new BitmapImage(new Uri(camera.File))
            //};

            ////model.Material = PhongMaterials.Yellow;

            ////Viewport.Items.Add(model);
            //models.Add(model);

            var linemodel = new LineGeometryModel3D();
            var lb = new LineBuilder();

            lb.AddLine(camera.CameraCenter.ToVector3(), p1.ToVector3());
            lb.AddLine(camera.CameraCenter.ToVector3(), p2.ToVector3());
            lb.AddLine(camera.CameraCenter.ToVector3(), p3.ToVector3());
            lb.AddLine(camera.CameraCenter.ToVector3(), p4.ToVector3());
            lb.AddLine(p1.ToVector3(), p2.ToVector3());
            lb.AddLine(p2.ToVector3(), p3.ToVector3());
            lb.AddLine(p3.ToVector3(), p4.ToVector3());
            lb.AddLine(p4.ToVector3(), p1.ToVector3());
            lb.AddLine(p1.ToVector3(), p3.ToVector3());
            lb.AddLine(p2.ToVector3(), p4.ToVector3());

            linemodel.Geometry = lb.ToLineGeometry3D();
            linemodel.Color = LUT.GetRandomColor();

            linemodel.Name = "PlanarCamera_" + Path.GetFileName(camera.File).Split('.').First() + "_" + camera.Id;
            
            models.Add(linemodel);

            MeshBuilder mb = new MeshBuilder();
            MeshGeometryModel3D modelCameraCenter = new MeshGeometryModel3D();

            mb.AddSphere(camera.CameraCenter.ToVector3(), 0.03);
            mb.AddSphere(p1.ToVector3(), 0.03);
            modelCameraCenter.Geometry = mb.ToMeshGeometry3D();

            modelCameraCenter.Material = new PhongMaterial()
            {
                DiffuseColor = linemodel.Color,
                AmbientColor = linemodel.Color
            };


            //models.Add(modelCameraCenter);

            return models.ToArray();
        }

        public static GeometryModel3D[] GetCameraSpherical(CameraPosition camera, double radius = 1)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            MeshBuilder mb = new MeshBuilder();
            MeshGeometryModel3D model = new MeshGeometryModel3D();

            mb.AddSphere(camera.CameraCenter.ToVector3(), radius);
            var geometry = mb.ToMeshGeometry3D();

            for (int i = 0; i < geometry.TextureCoordinates.Count; i++)
            {
                var v = geometry.TextureCoordinates[i];
                v.X = 1 - v.X;
                geometry.TextureCoordinates[i] = v;
            }

            model.Geometry = geometry;
            model.Geometry.Colors = new Color4Collection(geometry.TextureCoordinates.Select(x => x.ToColor4()));

            model.Transform = new TranslateTransform3D(0, 0, 0);

            model.Material = PhongMaterials.Blue;

            model.Name = "SphericalCamera_" + camera.Name + "_" + camera.Id;

            models.Add(model);

            var linemodel = new LineGeometryModel3D();
            var lb = new LineBuilder();
            //lb.AddLine(camera.CameraCenter, camera.CameraCenter + CameraPosition.Rotate(camera.Orientation, new Vector3(1, 0, 0)));
            //linemodel.Geometry = lb.ToLineGeometry3D();
            //linemodel.Color = new Color(255, 0, 0);

            //models.Add(linemodel);


            //linemodel = new LineGeometryModel3D();
            //lb = new LineBuilder();
            //lb.AddLine(camera.CameraCenter, camera.CameraCenter + CameraPosition.Rotate(camera.Orientation, new Vector3(0, 1, 0)));
            //linemodel.Geometry = lb.ToLineGeometry3D();
            //linemodel.Color = new Color(0, 255, 0);

            //models.Add(linemodel);

            //linemodel = new LineGeometryModel3D();
            //lb = new LineBuilder();
            //lb.AddLine(camera.CameraCenter, camera.CameraCenter + CameraPosition.Rotate(camera.Orientation, new Vector3(0, 0, 1)));
            //linemodel.Geometry = lb.ToLineGeometry3D();
            //linemodel.Color = new Color(0, 0, 255);

            //models.Add(linemodel);

            linemodel = new LineGeometryModel3D();
            lb = new LineBuilder();
            lb.AddLine(camera.CameraCenter.ToVector3(), (camera.CameraCenter + camera.Orientation.Axis).ToVector3());
            linemodel.Geometry = lb.ToLineGeometry3D();
            linemodel.Color = new Color(255, 255, 0);

            linemodel.Name = "Orientation_SphericalCamera_" + camera.Name + "_" + camera.Id;

            models.Add(linemodel);

            return models.ToArray();
        }
    }
}
