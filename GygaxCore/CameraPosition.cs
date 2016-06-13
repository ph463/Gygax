using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using SharpDX;

namespace GygaxCore
{
    public class CameraPosition
    {
        public CameraType Type;

        public string File;

        public string Name;

        public int Id;

        public double FocalLength;

        public Quaternion Orientation;

        public Vector3 CameraCenter;

        public double RadialDistortion;

        public int Width;

        public int Height;

        [XmlIgnore]
        private Image<Bgr, Byte> _image;

        [XmlIgnore]
        public Image<Bgr, Byte> Image
        {
            get { return _image; }
            set
            {
                _image = value;
                Width = _image.Width;
                Height = _image.Height;
            }
        }

        public Vector3 Normal;

        public Matrix3x3 Basis;

        public double OpeningAngleDiagonal;
        public double OpeningAngleHorizontalFrom = 0;
        public double OpeningAngleHorizontalTo = 2.0 * Math.PI;
        public double OpeningAngleVerticalFrom = Math.PI / 2.0;
        public double OpeningAngleVerticalTo = -Math.PI / 2.0;

        public enum CameraType { Planar, Spherical }

        public enum Direction { TopLeft, TopRight, BottomLeft, BottomRight };

        public static Vector3 GetCornerPointToAxis(CameraPosition cameraPosition, Quaternion axis, Direction direction)
        {
            // Orientation in Viewing direction
            var a1 = cameraPosition.Width / 2;
            var a2 = cameraPosition.Height / 2;
            var a3 = (double)cameraPosition.FocalLength;

            var angleY = Math.Atan(a1 / a3);
            var angleX = Math.Atan(a2 / a3);

            // Set sign for corner
            switch (direction)
            {
                default:
                case Direction.TopLeft:
                    break;
                case Direction.TopRight:
                    angleX = -angleX;
                    break;
                case Direction.BottomLeft:
                    angleY = -angleY;
                    break;
                case Direction.BottomRight:
                    angleX = -angleX;
                    angleY = -angleY;
                    break;
            }

            // Rotation matrices
            var xm = new SharpDX.Matrix
            {
                M11 = 1,
                M22 = (float)Math.Cos(angleX),
                M32 = (float)Math.Sin(angleX),
                M23 = (float)-Math.Sin(angleX),
                M33 = (float)Math.Cos(angleX)
            };

            var ym = new SharpDX.Matrix
            {
                M11 = (float)Math.Cos(angleY),
                M31 = (float)-Math.Sin(angleY),
                M22 = 1,
                M13 = (float)Math.Sin(angleY),
                M33 = (float)Math.Cos(angleY)
            };

            // Rotate the point by multiplying with quaternion and conjugate of quaternion
            var rot = Quaternion.RotationMatrix(xm) * Quaternion.RotationMatrix(ym) * axis;

            return Rotate(rot, new Vector3(0, 0, 1));
        }

        public static Vector3 Rotate(Quaternion q, Vector3 v)
        {
            var conj = new Quaternion(q.ToArray());
            conj.Conjugate();

            Quaternion rotatedVector = conj * new Quaternion(v, 0) * q;

            return new Vector3(rotatedVector.X, rotatedVector.Y, rotatedVector.Z);
        }
    }
}
