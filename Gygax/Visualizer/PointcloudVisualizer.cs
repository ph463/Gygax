using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using GygaxCore;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;

namespace GygaxVisu.Visualizer
{
    public class PointcloudVisualizer : Visualizer
    {
        public static GeometryModel3D[] GetModels(Pointcloud pointcloud)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            PointGeometryModel3D model = new PointGeometryModel3D();

            // This one is important, otherwise it will be just black
            model.Color = Color.White;

            model.Geometry = (PointGeometry3D) pointcloud.Data;

            models.Add(model);

            return models.ToArray();
        }

        public static GeometryModel3D[] GetModels(PointGeometry3D pointcloud)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            PointGeometryModel3D model = new PointGeometryModel3D();

            // This one is important, otherwise it will be just black
            model.Color = Color.White;

            model.Geometry = pointcloud;

            models.Add(model);

            return models.ToArray();
        }
    }
}
