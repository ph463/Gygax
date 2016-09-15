using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace GygaxCore.DataStructures
{
    [Serializable]
    public class CameraPosition
    {
        public CameraType Type;

        public string File;

        public string Name;

        public int Id;

        public double FocalLength;

        private Quaternion _orientation;

        public Quaternion Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                _orientation.Normalize();

                Normal = Rotate(_orientation, new Vector3D(0,0,1));
                Normal.Normalize();

                Basis = new Matrix3x3
                {
                    Row1 = Vector3.Normalize(Rotate(_orientation, new Vector3D(1,0,0)).ToVector3()),
                    Row2 = Vector3.Normalize(Rotate(_orientation, new Vector3D(0, 1, 0)).ToVector3()),
                    Row3 = Vector3.Normalize(Rotate(_orientation, new Vector3D(0, 0, 1)).ToVector3())
                };
            }
        }
        
        public double ImageDiagonal { get; set; }

        public Vector3D CameraCenter;

        public double RadialDistortion;

        public int Width { get; set; }

        public int Height { get; set; }

        [XmlIgnore]
        [NonSerialized]
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
                ImageDiagonal = Math.Sqrt(Math.Pow(_image.Width, 2) + Math.Pow(_image.Height, 2));
            }
        }

        public Vector3D Normal { get; private set; }

        [NonSerialized] public Matrix3x3 Basis;

        public double OpeningAngleDiagonal;
        public double OpeningAngleHorizontalFrom = 0;
        public double OpeningAngleHorizontalTo = 2.0 * Math.PI;
        public double OpeningAngleVerticalFrom = Math.PI / 2.0;
        public double OpeningAngleVerticalTo = -Math.PI / 2.0;

        public enum CameraType { Planar, Spherical }

        public enum Direction { TopLeft, TopRight, BottomLeft, BottomRight };

        public static Vector3D GetCornerPointToAxis(CameraPosition cameraPosition, Quaternion axis, Direction direction)
        {
            // Orientation in Viewing direction
            var a1 = cameraPosition.Image.Width / 2;
            var a2 = cameraPosition.Image.Height / 2;
            var a3 = cameraPosition.FocalLength;

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
            var xm = new Matrix3D()
            {
                M11 = 1,
                M22 = Math.Cos(angleX),
                M32 = Math.Sin(angleX),
                M23 = -Math.Sin(angleX),
                M33 = Math.Cos(angleX)
            };

            var ym = new Matrix3D()
            {
                M11 = Math.Cos(angleY),
                M31 = -Math.Sin(angleY),
                M22 = 1,
                M13 = Math.Sin(angleY),
                M33 = Math.Cos(angleY)
            };

            // Rotate the point by multiplying with quaternion and conjugate of quaternion
            var rot = TransformToQuaternion(xm) * TransformToQuaternion(ym) * axis;

            return Rotate(rot, new Vector3D(0, 0, 1));
        }

        public static Vector3D Rotate(Quaternion q, Vector3D v)
        {
            var conj = new System.Windows.Media.Media3D.Quaternion(q.X, q.Y, q.Z, q.W);
            conj.Conjugate();

            var rotatedVector = conj * new System.Windows.Media.Media3D.Quaternion(v.X, v.Y, v.Z, 0) * q;

            return new Vector3D(rotatedVector.X, rotatedVector.Y, rotatedVector.Z);
        }

        public static Quaternion TransformToQuaternion(Matrix3D transform)
        {
            var scale = Math.Sqrt((transform.M11 * transform.M11) + (transform.M21 * transform.M21) + (transform.M31 * transform.M31));
            var invScale = 1.0 / scale;

            // Be very carefull here with the correct addressing!

            var rotationmatrix = new Matrix3D()
            {
                M11 = (transform.M11 * invScale),
                M12 = (transform.M21 * invScale),
                M13 = (transform.M31 * invScale),

                M21 = (transform.M12 * invScale),
                M22 = (transform.M22 * invScale),
                M23 = (transform.M32 * invScale),

                M31 = (transform.M13 * invScale),
                M32 = (transform.M23 * invScale),
                M33 = (transform.M33 * invScale),
                M44 = 1
            };

            return RotationMatrix(rotationmatrix);
        }
        
        public void Transform(Matrix3D transform)
        {
            var tg = new MatrixTransform3D(transform);
            CameraCenter = tg.Transform(CameraCenter.ToPoint3D()).ToVector3D();
            
            var cameraOrientation = new Quaternion(Orientation.X, Orientation.Y, Orientation.Z, Orientation.W);

            var left = cameraOrientation;
            var right = TransformToQuaternion(transform);
            
            Orientation = left*right;
        }

        public CameraPosition Duplicate()
        {
            return new CameraPosition()
            {
                CameraCenter = new Vector3D(CameraCenter.X, CameraCenter.Y, CameraCenter.Z),
                Orientation = new Quaternion(Orientation.X, Orientation.Y, Orientation.Z, Orientation.W),
                File = File,
                FocalLength = FocalLength,
                RadialDistortion = RadialDistortion,
                Width = Width,
                Height = Height,
                OpeningAngleDiagonal = OpeningAngleDiagonal
            };
        }

        public static Quaternion RotationMatrix(Matrix3D matrix)
        {
            double sqrt;
            double half;
            var scale = matrix.M11 + matrix.M22 + matrix.M33;

            var result = new Quaternion();

            if (scale > 0.0)
            {
                sqrt = Math.Sqrt(scale + 1.0);
                result.W = sqrt * 0.5;
                sqrt = 0.5 / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5 / sqrt;

                result.X = 0.5 * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5 / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5 * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;
            }
            else
            {
                sqrt = Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5 / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5 * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }

            return result;
        }

        public static Quaternion RotationAxis(Vector3D axis, double angle)
        {
            axis.Normalize();

            var half = angle * 0.5;
            var sin = Math.Sin(half);
            var cos = Math.Cos(half);

            Quaternion result = new Quaternion
            {
                X = axis.X*sin,
                Y = axis.Y*sin,
                Z = axis.Z*sin,
                W = cos
            };
            
            return result;
        }
    }
}
