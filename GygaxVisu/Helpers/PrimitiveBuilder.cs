using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace GygaxVisu
{
    public class PrimitiveBuilder
    {

        public static MeshGeometry3D GetRect(Vector3 center, float width, float height)
        {
            var widthOrientation = new Vector3(0, 0, 1);
            var heightOrientation = new Vector3(0, 1, 0);

            return GetRect(center, width, height, widthOrientation, heightOrientation);
        }

        public static MeshGeometry3D GetRect(Vector3 center, float width, float height, Vector3 widthOrientation, Vector3 heightOrientation)
        {
            var positions = new Vector3Collection();
            var triangleIndices = new HelixToolkit.Wpf.SharpDX.Core.IntCollection();

            var normals = new Vector3Collection();
            //private Vector3Collection tangents;
            //private Vector3Collection bitangents;
            var textureCoordinates = new Vector2Collection();

            positions.Add(center - widthOrientation * width / 2 - heightOrientation * height / 2);
            positions.Add(center + widthOrientation * width / 2 - heightOrientation * height / 2);
            positions.Add(center + widthOrientation * width / 2 + heightOrientation * height / 2);
            positions.Add(center - widthOrientation * width / 2 + heightOrientation * height / 2);

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

        public static MeshGeometry3D GetCube(Vector3 minimum, Vector3 maximum)
        {
            var positions = new Vector3Collection();
            var triangleIndices = new HelixToolkit.Wpf.SharpDX.Core.IntCollection();

            var normals = new Vector3Collection();
            var textureCoordinates = new Vector2Collection();

            positions.Add(new Vector3(minimum.X, minimum.Y, minimum.Z)); //0
            positions.Add(new Vector3(maximum.X, minimum.Y, minimum.Z)); //1
            positions.Add(new Vector3(minimum.X, maximum.Y, minimum.Z)); //2
            positions.Add(new Vector3(minimum.X, minimum.Y, maximum.Z)); //3
            positions.Add(new Vector3(maximum.X, maximum.Y, minimum.Z)); //4
            positions.Add(new Vector3(maximum.X, minimum.Y, maximum.Z)); //5
            positions.Add(new Vector3(minimum.X, maximum.Y, maximum.Z)); //6
            positions.Add(new Vector3(maximum.X, maximum.Y, maximum.Z)); //7

            triangleIndices.AddRange(new[] { 0, 5, 3, 0, 1, 5 });
            triangleIndices.AddRange(new[] { 1, 7, 5, 1, 4, 7 });
            triangleIndices.AddRange(new[] { 4, 7, 2, 7, 6, 2 });
            triangleIndices.AddRange(new[] { 6, 3, 0, 6, 0, 2 });
            triangleIndices.AddRange(new[] { 6, 3, 5, 5, 7, 6 });
            triangleIndices.AddRange(new[] { 0, 1, 2, 2, 1, 4 });

            for (int i = 0; i < positions.Count; i++)
            {
                normals.Add(new Vector3(1, 0, 0));
                textureCoordinates.Add(new Vector2(0, 0));
            }


            return new MeshGeometry3D()
            {
                Positions = positions,
                Indices = triangleIndices,
                Normals = normals,
                TextureCoordinates = textureCoordinates
            };
        }

        public static MeshGeometry3D GetRect(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
