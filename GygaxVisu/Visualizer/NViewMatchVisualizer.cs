using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;

namespace GygaxVisu.Visualizer
{
    public class NViewMatchVisualizer:Visualizer
    {
        public static GeometryModel3D[] GetModels(NViewMatch nvm)
        {
            var showCameras = true;

            var tg = new MatrixTransform3D(nvm.Transform);

            List<GeometryModel3D> models = new List<GeometryModel3D>();

            if (showCameras)
            {
                foreach (var cameraPosition in nvm.CameraPositions)
                {
                    var pos = GetCamera(cameraPosition);

                    foreach (var c in pos)
                    {
                        c.Transform = tg;
                        models.Add(c);
                    }
                }
            }

            if (nvm.Patches.Count <= 0)
                return models.ToArray();

            PointGeometryModel3D model = new PointGeometryModel3D();

            model.Color = Color.White;

            model.Geometry = Pointcloud.ConvertToPointGeometry3D(
                nvm.Patches.Select(patch => new PclWrapper.Points
                {
                    x = patch.Position.X,
                    y = patch.Position.Y,
                    z = patch.Position.Z,
                    r = (byte)patch.Color.Red,
                    g = (byte)patch.Color.Green,
                    b = (byte)patch.Color.Blue,
                    a = 255
                }).ToArray());

            models.Add(model);

            model.Transform = tg;

            return models.ToArray();
        }
    }
}
