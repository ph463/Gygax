using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Media3D;
using GygaxCore;
using GygaxCore.Ifc;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace GygaxVisu.Visualizer
{
    class IfcVisualizer : Visualizer
    {
        public static GeometryModel3D[] GetModels(IfcViewerWrapper ifcModel)
        {
            var ifc = (IfcViewerWrapper)ifcModel.Data;
            var modelList = GetItems(ifc);

            return modelList.Cast<GeometryModel3D>().ToArray();
        }

        public static IfcMeshGeometryModel3D[] GetItems(IfcViewerWrapper model)
        {
            var element = model.RootIfcItem;
            var modelList = new List<IfcMeshGeometryModel3D>();

            Draw(element, ref modelList);

            foreach (var model3D in modelList)
            {
                model3D.Material = new PhongMaterial()
                {
                    DiffuseColor = new Color4(0.1f, 0.1f, 0.15f, 1.0f),
                    AmbientColor = new Color4(0.1f, 0.1f, 0.15f, 1.0f),
                    SpecularColor = new Color4(0.1f, 0.1f, 0.15f, 1.0f),
                    EmissiveColor = new Color4(0.1f, 0.1f, 0.02f, 0.5f),
                    SpecularShininess = 0.5f
                };
            }
            
            return modelList.ToArray();
        }

        public static void Draw(IFCItem element, ref List<IfcMeshGeometryModel3D> modelList)
        {
            while (element != null)
            {
                if (element.child != null)
                {
                    Draw(element.child, ref modelList);
                }

                if (!(element.noPrimitivesForFaces > 0))
                {
                    element = element.next;
                    continue;
                }

                MeshGeometry3D geometry = new MeshGeometry3D()
                {
                    Positions = new Vector3Collection(),
                    Normals = new Vector3Collection(),
                    Indices = new IntCollection(),
                    TextureCoordinates = new Vector2Collection()
                };

                var positions = new Vector3Collection();
                var normals = new Vector3Collection();
                
                //ToDo: Depending on if the model is modelled in meter or millimeter this divident has to be changed
                var divident = 1000;

                for (int i = 0; i < element.noVerticesForFaces; i++)
                {
                    var offset = i * 6;
                    positions.Add(new SharpDX.Vector3(
                        element.verticesForFaces[offset + 0] / divident,
                        element.verticesForFaces[offset + 2] / divident,
                        -element.verticesForFaces[offset + 1] / divident
                        ));

                    normals.Add(new SharpDX.Vector3(
                        element.verticesForFaces[offset + 3],
                        element.verticesForFaces[offset + 5],
                        -element.verticesForFaces[offset + 4]
                        ));
                }

                for (int i = 0; i < element.indicesForFaces.Length; i++)
                {
                    geometry.Indices.Add(i);

                    geometry.Positions.Add(positions[(int)element.indicesForFaces[i]]);
                    geometry.Normals.Add(normals[(int)element.indicesForFaces[i]]);

                    //just needs to be filled with something
                    geometry.TextureCoordinates.Add(new Vector2(0,0));
                }

                modelList.Add(new IfcMeshGeometryModel3D()
                {
                    Geometry = geometry,
                    IfcName = element.name
                });

                element = element.next;
            }
        }
    }
}
