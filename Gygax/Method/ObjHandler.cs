using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using GygaxVisu.Controls;
using HelixToolkit.Wpf.SharpDX;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using GygaxCore.Ifc;
using SharpDX;

namespace GygaxVisu
{
    public class ObjHandler
    {
        public void Export(IfcMeshGeometryModel3D[] modelList, string path, string texturePath="")
        {
            var objFile = path + @"\bridge1.obj";
            
            var mtlFile = path + @"\bridge1.mtl";



            int offset = 0;
            
            var objWriter = new StreamWriter(objFile);
            var mtlWriter = new StreamWriter(mtlFile);

            objWriter.WriteLine("mtllib " + Path.GetFileName(mtlFile));
            objWriter.WriteLine();
            objWriter.WriteLine("o " + "Bridge1");

            var translate = -findPointOfGravity(modelList);

            foreach (var element in modelList)
            {
                var modelName = IfcViewerWrapper.GetValidPathName(element.IfcName);

                var tmpFile = path + @"\" + modelName + ".obj";

                List<string> vList = new List<string>();
                List<string> vtList = new List<string>();
                List<string> vnList = new List<string>();

                List<string> fList = new List<string>();

                var exp = new ObjExporter(tmpFile);
                exp.ExportNormals = true;

                exp.ExportMesh((MeshGeometry3D) element.Geometry, Transform3D.Identity);
                exp.Close();

                using (StreamReader sr = File.OpenText(tmpFile))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        switch (s.Split(' ')[0])
                        {
                            case "v":
                                var e = s.Split(' ');
                                var sl = "v "+ (Convert.ToDouble(e[1]) + translate.X) + " " + (-Convert.ToDouble(e[3]) + translate.Y) + " " + (Convert.ToDouble(e[2]) + translate.Z);
                                vList.Add(sl);
                                break;
                            case "vt":
                                vtList.Add(s);
                                break;
                            case "vn":
                                vnList.Add(s);
                                break;
                            case "f":
                                var split = s.Split(' ');
                                var st = "f ";
                                for (int i = 3; i > 0; i--)
                                {
                                    var ad = split[i].Split('/');
                                    st += (Convert.ToInt32(ad[0]) + offset) + "/" +
                                            (Convert.ToInt32(ad[2]) + offset) +
                                            "/" + (Convert.ToInt32(ad[1]) + offset) + " ";
                                }
                                fList.Add(st);
                                break;
                        }
                    }
                }

                File.Delete(tmpFile);

                var tmpMtlFile = Path.GetFileName(tmpFile).Split('.');
                tmpMtlFile[tmpMtlFile.Length - 1] = "mtl";
                File.Delete(Path.GetDirectoryName(tmpFile) + @"\" + String.Join(".",tmpMtlFile));

                offset += vList.Count;

                objWriter.WriteLine();
                objWriter.WriteLine("g " + modelName);

                foreach (var line in vList)
                {
                    objWriter.WriteLine(line);
                }

                foreach (var line in vtList)
                {
                    objWriter.WriteLine(line);
                }

                foreach (var line in vnList)
                {
                    objWriter.WriteLine(line);
                }

                objWriter.WriteLine();
                objWriter.WriteLine("usemtl material_" + modelName);

                foreach (var f in fList)
                {
                    objWriter.WriteLine(f);
                }
                
                mtlWriter.WriteLine("newmtl material_" + modelName + "\n" +
                                //"Ka 1.000 1.000 1.000     # white" + "\n" +
                                //"Kd 1.000 1.000 1.000     # white" + "\n" +
                                "Ks 0.000 0.000 0.000     # black (off)" + "\n" +
                                "map_Kd " + modelName + ".jpg" + "\n");

                var textureFile = texturePath + @"\" + modelName + ".jpg";

                if(!texturePath.Equals("") && File.Exists(textureFile))
                    File.Copy(textureFile, path + @"\" + Path.GetFileName(textureFile));
            }

            objWriter.Close();
            mtlWriter.Close();
        }

        private Vector3D findPointOfGravity(IfcMeshGeometryModel3D[] modelList)
        {

            var m = new Vector3D(0,0,0);
            int c=0;

            foreach (var model in modelList)
            {
                foreach (var pos in model.Geometry.Positions)
                {
                    m += pos.ToVector3D();
                    c++;
                }
                
            }

            return m/c;
        }
    }
}
