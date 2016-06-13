using System.Collections.Generic;
using System.Linq;
using GygaxCore;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;

namespace GygaxVisu.Visualizer
{
    public class NViewMatchVisualizer:Visualizer
    {
        public static GeometryModel3D[] GetModels(NViewMatch nvm)
        {
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            foreach (var cameraPosition in nvm.CameraPositions)
            {
                models.AddRange(GetCamera(cameraPosition));
            }

            if(nvm.Patches.Count <= 0)
                return models.ToArray();

            PointGeometryModel3D model = new PointGeometryModel3D();

            model.Color = Color.White;

            model.Geometry = Pointcloud.ConvertToPointGeometry3D(
                nvm.Patches.Select(patch => new PclWrapper.Points()
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

            return models.ToArray();
        }

    }
}
