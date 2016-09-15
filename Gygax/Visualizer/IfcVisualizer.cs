﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using GygaxCore.Ifc;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace GygaxVisu.Visualizer
{
    public class IfcVisualizer : Visualizer
    {
        public static GeometryModel3D[] GetModels(IfcViewerWrapper ifcModel)
        {
            var ifc = (IfcViewerWrapper)ifcModel.Data;
            List<GeometryModel3D> models = new List<GeometryModel3D>();

            models.AddRange(GetItems(ifcModel));

            return models.ToArray();
        }

        public static IfcMeshGeometryModel3D[] GetItems(IfcViewerWrapper model, bool loadTexture = true, string[] filter = null)
        {
            var element = model.RootIfcItem;
            var modelList = new List<IfcMeshGeometryModel3D>();

            Draw(element, ref modelList, filter);

            foreach (var model3D in modelList)
            {
                model3D.Mapper = new UvMapper((MeshGeometry3D)model3D.Geometry, modelList.ToArray())
                {
                    TextureFilename = Path.GetDirectoryName(model.IfcFile) + @"\..\Textures\" + IfcViewerWrapper.GetValidPathName(model3D.IfcName) + ".jpg",
                    TextureHeight = model3D.TextureHeight,
                    TextureWidth = model3D.TextureWidth
                };

                if (loadTexture && File.Exists(model3D.Mapper.TextureFilename))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(model3D.Mapper.TextureFilename);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    model3D.Material = new PhongMaterial()
                    {
                        AmbientColor = new SharpDX.Color4(0f, 0f, 0f, 1f),
                        DiffuseColor = new SharpDX.Color4(1f, 1f, 1f, 1f),
                        DiffuseMap = bitmap
                    };
                }
                else if (true)
                {
                    var returnColor = new SharpDX.Color3(.8f, .8f, .8f);

                    if (model3D.IfcName.Contains("pierCap"))
                    {
                        returnColor = new SharpDX.Color3(0, 0.8f, 0);
                    }
                    else if (model3D.IfcName.Contains("deck"))
                    {
                        returnColor = new SharpDX.Color3(0.8f, 0, 0);
                    }
                    else if (model3D.IfcName.Contains("Concrete-Round-Column"))
                    {
                        returnColor = new SharpDX.Color3(0.8f, 0.8f, 0);
                    }
                    else if (model3D.IfcName.Contains("Surface"))
                    {
                        returnColor = new SharpDX.Color3(0.87f, 0.72f, .53f);
                    }
                    else if (model3D.IfcName.Contains("Foundation"))
                    {
                        returnColor = new SharpDX.Color3(0.12f, 0.56f, 0.8f);
                    }
                    else if (model3D.IfcName.Contains("Floor"))
                    {
                        returnColor = new SharpDX.Color3(0.12f, 0.56f, 0.8f);
                    }
                    else if (model3D.IfcName.Contains("Basic Wall"))
                    {
                        returnColor = new SharpDX.Color3(0.64f, 0.11f, 1f);
                    }
                    else
                    {
                        returnColor = new SharpDX.Color3(0.8f, 0, 0);
                    }

                    model3D.Material = new PhongMaterial()
                    {
                        AmbientColor = new SharpDX.Color4(returnColor.Red*.3f, returnColor.Green * .3f, returnColor.Blue * .3f, 1f),
                        DiffuseColor = new SharpDX.Color4(returnColor, 1f)
                    };
                }
                else
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
                
            }

            return modelList.ToArray();
        }

        public static void Draw(IFCItem element, ref List<IfcMeshGeometryModel3D> modelList, string[] filter)
        {
            while (element != null)
            {
                if (element.child != null)
                {
                    Draw(element.child, ref modelList, filter);
                }

                if (!(element.noPrimitivesForFaces > 0))
                {
                    element = element.next;
                    continue;
                }

                if (filter != null)
                {
                    var index = Array.FindIndex(filter,
                        x => x.Equals(element.name, StringComparison.InvariantCultureIgnoreCase));

                    if (filter.Any() && index == -1)
                    {
                        element = element.next;
                        continue;
                    }
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
                    //geometry.TextureCoordinates.Add(new Vector2(0,0));
                }

                var uvm = new UvMapper(geometry, modelList.ToArray());
                geometry.TextureCoordinates = uvm.GenerateIndexes();

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
