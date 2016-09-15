using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace GygaxVisu
{
    public class PrimitiveBuilder
    {
        public MeshGeometry3D GetRect(Vector3 center, float width, float height)
        {
            var positions = new Vector3Collection();
            var triangleIndices = new HelixToolkit.Wpf.SharpDX.Core.IntCollection();

            var normals = new Vector3Collection();
            //private Vector3Collection tangents;
            //private Vector3Collection bitangents;
            var textureCoordinates = new Vector2Collection();

            var widthOrientation = new Vector3(0, 0, 1);
            var heightOrientation = new Vector3(0, 1, 0);

            positions.Add(center);
            positions.Add(center + widthOrientation * width);
            positions.Add(center + widthOrientation * width + heightOrientation * height);
            positions.Add(center + heightOrientation * height);
            
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            
            textureCoordinates.Add(new Vector2(1, 1));
            textureCoordinates.Add(new Vector2(0, 1));
            textureCoordinates.Add(new Vector2(0, 0));
            textureCoordinates.Add(new Vector2(1, 0));

            triangleIndices.Add(2);
            triangleIndices.Add(1);
            triangleIndices.Add(0);

            triangleIndices.Add(0);
            triangleIndices.Add(3);
            triangleIndices.Add(2);

            return new MeshGeometry3D()
            {
                Positions = positions,
                Indices = triangleIndices,
                Normals = normals,
                TextureCoordinates = textureCoordinates
            };
        }

        public MeshGeometry3D GetRect(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var positions = new Vector3Collection();
            var triangleIndices = new HelixToolkit.Wpf.SharpDX.Core.IntCollection();

            var normals = new Vector3Collection();
            //private Vector3Collection tangents;
            //private Vector3Collection bitangents;
            var textureCoordinates = new Vector2Collection();

            var widthOrientation = new Vector3(0, 0, 1);
            var heightOrientation = new Vector3(0, 1, 0);

            positions.Add(p0);
            positions.Add(p1);
            positions.Add(p2);
            positions.Add(p3);

            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));


            textureCoordinates.Add(new Vector2(0, 0));
            textureCoordinates.Add(new Vector2(1, 0));
            textureCoordinates.Add(new Vector2(1, 1));
            textureCoordinates.Add(new Vector2(0, 1));

            triangleIndices.Add(2);
            triangleIndices.Add(1);
            triangleIndices.Add(0);

            triangleIndices.Add(0);
            triangleIndices.Add(3);
            triangleIndices.Add(2);

            return new MeshGeometry3D()
            {
                Positions = positions,
                Indices = triangleIndices,
                Normals = normals,
                TextureCoordinates = textureCoordinates
            };
        }
    }
}
