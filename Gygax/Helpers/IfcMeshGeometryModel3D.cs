using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace GygaxVisu
{
    public class IfcMeshGeometryModel3D: MeshGeometryModel3D
    {
        public string IfcName;
        public int TextureWidth;
        public int TextureHeight;
        public UvMapper Mapper;
    }
}
