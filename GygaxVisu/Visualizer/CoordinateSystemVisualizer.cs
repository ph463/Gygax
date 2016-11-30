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
    public class CoordinateSystemVisualizer:Visualizer
    {
        public static GeometryModel3D[] GetModels(CoordinateSystem cs)
        {
            var tg = new MatrixTransform3D(cs.Transformation);

            List<GeometryModel3D> models = new List<GeometryModel3D>();

            int i = 1;
            
            var scale = (cs.Transformation.ToMatrix().Row1.Length() + 
                cs.Transformation.ToMatrix().Row2.Length() +
                cs.Transformation.ToMatrix().Row3.Length())/3;

            foreach (var correspondence in cs.Correspondences)
            {
                var m = ViewportHelper.GetSphere(correspondence.LocalCoordinateSystem, 0.1/scale, new PhongMaterial()
                {
                    AmbientColor = new Color(LUT.Colors[i, 0], LUT.Colors[i, 1], LUT.Colors[i, 2]),
                    DiffuseColor = new Color(LUT.Colors[i, 0], LUT.Colors[i, 1], LUT.Colors[i, 2])
                });

                m.Name = "Correspondence" + (i - 1) + "LocalCoordinateSystem";
                m.Transform = tg;
                models.Add(m);

                var n = ViewportHelper.GetSphere(correspondence.ParentCoordinateSystem, 0.08, new PhongMaterial()
                {
                    AmbientColor = new Color(LUT.Colors[i, 0]-5, LUT.Colors[i, 1]-5, LUT.Colors[i, 2]-5),
                    DiffuseColor = new Color(LUT.Colors[i, 0]-5, LUT.Colors[i, 1]-5, LUT.Colors[i, 2]-5)
                });

                n.Name = "Correspondence" + (i - 1) + "ParentCoordinateSystem";
                models.Add(n);

                i++;
            }
            
            return models.ToArray();
        }
    }
}
