using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using GygaxCore.Ifc;
using GygaxVisu.Helpers;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using NLog;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace GygaxVisu.Visualizer
{
    public class IfcVisualizer : Visualizer
    {
        public static GeometryModel3D[] GetModels(IfcViewerWrapper ifcModel)
        {
            var ifc = (IfcViewerWrapper) ifcModel.Data;
            var modelList = GetItems(ifc);

            return modelList.Cast<GeometryModel3D>().ToArray();
        }

        public static TreeViewItem GetTreeItems(IfcViewerWrapper ifcModel, GeometryModel3D[] models)
        {
            var treeItem = new TreeViewItem()
            {
                Header = ifcModel.Name,
                DataContext = ifcModel
            };

            if (models == null) return null;

            foreach (var model in models)
            {
                var subItem = new TreeViewItem()
                {
                    Header = model.Name,
                    DataContext = model
                };

                var ifcTree = ((IfcMeshGeometryModel3D) model).IfcTreeNode;
                addSubtree(ref subItem, ifcTree);

                var hideItem = new TreeViewItem()
                {
                    Header = "hide",
                };

                hideItem.MouseUp += delegate(object sender, MouseButtonEventArgs args)
                {
                    if(model.Visibility == Visibility.Visible)
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

        private static void addSubtree(ref TreeViewItem viewTree, TreeNode<TreeElement> ifcTree)
        {
            if (ifcTree == null || ifcTree.Children == null)
                return;

            foreach (var child in ifcTree.Children)
            {
                var t = new TreeViewItem()
                {
                    Header = child.Value.Key + " " + child.Value.Value + " " + child.Value.GlobalId,
                    DataContext = child.Value
                };

                addSubtree(ref t, child);

                viewTree.Items.Add(t);
            }
        }

        public static IfcMeshGeometryModel3D[] GetItems(IfcViewerWrapper model, bool loadTexture = true, string[] filter = null)
        {
            var element = model.RootIfcItem;
            var modelList = new List<IfcMeshGeometryModel3D>();

            Draw(element, ref modelList, null);

            AssignIfcProperties(model.Tree, ref modelList);
            
            foreach (var model3D in modelList)
            {
                try
                {
                    model3D.Name = new string(model3D.IfcName.ToCharArray().Where(c => Char.IsLetter(c) || Char.IsNumber(c)).ToArray());
                }
                catch (Exception)
                {
                    LogManager.GetCurrentClassLogger().Info("Can not assign name " + model3D.IfcName + "to IFC element");
                }

                if (true)
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

        private static void AssignIfcProperties(TreeNode<TreeElement> modelTree, ref List<IfcMeshGeometryModel3D> modelList)
        {
            foreach (var modelTreeChild in modelTree.Children)
            {
                foreach (var ifcMeshGeometryModel3D in modelList)
                {
                    if(modelTreeChild.Value.GlobalId != null && modelTreeChild.Value.GlobalId.Equals(ifcMeshGeometryModel3D.IfcGlobalId))
                    {
                        ifcMeshGeometryModel3D.IfcTreeNode = modelTreeChild;
                        LogManager.GetCurrentClassLogger().Info(ifcMeshGeometryModel3D.IfcName + " tree node assigned");
                    }
                }

                if(modelTreeChild.Children.Count > 0)
                    AssignIfcProperties(modelTreeChild, ref modelList);
            }
        }

        private static void Draw(IFCItem element, ref List<IfcMeshGeometryModel3D> modelList, string[] filter)
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
                    geometry.TextureCoordinates.Add(new Vector2(0,0));
                }

                modelList.Add(new IfcMeshGeometryModel3D()
                {
                    Geometry = geometry,
                    IfcName = element.name,
                    IfcGlobalId = element.globalID
                });

                element = element.next;
            }
        }
    }
}
