using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Decompositions;
using Emgu.CV.Features2D;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using Matrix = Accord.Math.Matrix;

namespace GygaxCore
{
    /// <summary>
    /// http://nghiaho.com/?page_id=671
    /// </summary>
    public class CoordinateSystem
    {
        public bool ReferenceSystem;
        public CoordinateSystem ParentCoordinateSystem;

        public List<Correspondence> Correspondences = new List<Correspondence>();

        public struct Correspondence
        {
            public Vector3 ParentCoordinateSystem;
            public Vector3 LocalCoordinateSystem;
        }

        public Correspondence Centroid;

        public Quaternion Rotation
        {
            get
            {
                Quaternion returnValue;
                var rotationMatrix = RotationMatrix;
                Quaternion.RotationMatrix(ref rotationMatrix, out returnValue);
                return returnValue;
            }
        }

        public Vector3 ConvertToParentCoordinate(Vector3 localPoint)
        {
            return (MultiplyMatrixVector(RotationMatrix, localPoint) + Translation - Centroid.ParentCoordinateSystem) / (float)Scaling + Centroid.ParentCoordinateSystem;
        }

        public Vector3 ConvertToLocalCoordinate(Vector3 parentPoint)
        {
            var invertedRotationMatrix = RotationMatrix;
            invertedRotationMatrix.Transpose();

            return MultiplyMatrixVector(invertedRotationMatrix, parentPoint - Translation);
        }

        public static Vector3 MultiplyMatrixVector(Matrix3x3 m, Vector3 v)
        {
            return new Vector3(m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z, m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z, m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z);
        }

        public Matrix3x3 RotationMatrix => new Matrix3x3(
            (float)_matrix[0, 0], (float)_matrix[0, 1], (float)_matrix[0, 2],
            (float)_matrix[1, 0], (float)_matrix[1, 1], (float)_matrix[1, 2],
            (float)_matrix[2, 0], (float)_matrix[2, 1], (float)_matrix[2, 2]);

        private double[,] _matrix = new double[3,3];

        public Vector3 Translation;
        
        public float Error => FindError();

        public void CalculateHomography()
        {
            FindCentroid();
            FindOptimalRotation();
            FindTranslation();
            FindScaling();
        }

        private float FindError()
        {
            double v =
                Correspondences.Sum(
                    correspondence =>
                        Math.Pow(
                            (ConvertToParentCoordinate(correspondence.LocalCoordinateSystem) - correspondence.ParentCoordinateSystem).Length(), 2));

            return (float)v;
        }
        
        private void FindCentroid()
        {
            var n = Correspondences.Count;

            Centroid.LocalCoordinateSystem = Correspondences.Aggregate(Centroid.LocalCoordinateSystem,
                (current, correspondence) => current + correspondence.LocalCoordinateSystem/n);

            Centroid.ParentCoordinateSystem = Correspondences.Aggregate(Centroid.ParentCoordinateSystem,
                (current, correspondence) => current + correspondence.ParentCoordinateSystem/n);
        }

        private void FindOptimalRotation()
        {
            var h = new double[3,3];

            foreach (var correspondence in Correspondences)
            {
                var pa = correspondence.LocalCoordinateSystem - Centroid.LocalCoordinateSystem;
                var pb = correspondence.ParentCoordinateSystem - Centroid.ParentCoordinateSystem;

                var pam = Matrix.Create<double>(3, 1);
                pam[0, 0] = pa.X;
                pam[1, 0] = pa.Y;
                pam[2, 0] = pa.Z;

                var pbm = Matrix.Create<double>(1, 3);
                pbm[0, 0] = pb.X;
                pbm[0, 1] = pb.Y;
                pbm[0, 2] = pb.Z;

                var hl = Matrix.Add(h, Matrix.Multiply(pam, pbm));
                h = hl;
            }

            var svd = new SingularValueDecomposition(h);

            var r = Matrix.Multiply(svd.RightSingularVectors,Matrix.Transpose(svd.LeftSingularVectors));

            if (Matrix.Determinant(r) < 0)
            {
                var v = svd.RightSingularVectors;

                v[0, 2] *= -1;
                v[1, 2] *= -1;
                v[2, 2] *= -1;

                r = Matrix.Multiply(v, Matrix.Transpose(svd.LeftSingularVectors));
            }

            _matrix = r;
        }

        private void FindTranslation()
        {
            var pam = Matrix.Create<double>(3, 1);
            pam[0, 0] = Centroid.LocalCoordinateSystem.X;
            pam[1, 0] = Centroid.LocalCoordinateSystem.Y;
            pam[2, 0] = Centroid.LocalCoordinateSystem.Z;

            var pbm = Matrix.Create<double>(3, 1);
            pbm[0, 0] = Centroid.ParentCoordinateSystem.X;
            pbm[1, 0] = Centroid.ParentCoordinateSystem.Y;
            pbm[2, 0] = Centroid.ParentCoordinateSystem.Z;

            var a = Matrix.Add(Matrix.Multiply(-1, Matrix.Multiply(_matrix, pam)), pbm);
            
            Translation = new Vector3((float)a[0,0], (float)a[1, 0], (float)a[2, 0]);
        }

        public Double Scaling;

        private void FindScaling()
        {
            Scaling = 0.0;

            foreach (var correspondence in Correspondences)
            {
                var parentVectorLength =
                    (correspondence.ParentCoordinateSystem - Centroid.ParentCoordinateSystem).Length();

                var localVectorLength =
                    (correspondence.LocalCoordinateSystem - Centroid.LocalCoordinateSystem).Length();

                Scaling += localVectorLength/parentVectorLength;
            }

            Scaling /= Correspondences.Count;
        }

        public void AddTestelements()
        {
            Correspondences = new List<Correspondence>();

            const int n = 10;

            var t = Matrix.Random(3, 1);

            var svd = new SingularValueDecomposition(Matrix.Random(3, 3));
            var r = Matrix.Multiply(svd.LeftSingularVectors, svd.RightSingularVectors);

            if (Matrix.Determinant(r) < 0)
            {
                var v = svd.RightSingularVectors;

                v[0, 2] *= -1;
                v[1, 2] *= -1;
                v[2, 2] *= -1;

                r = Matrix.Multiply(v, Matrix.Transpose(svd.LeftSingularVectors));
            }

            var a = Matrix.Random(3, n);
            
            var tExpanded = t;

            for (int i = 0; i < n-1; i++)
                tExpanded = Matrix.InsertColumn(tExpanded, Matrix.GetColumn(t,0));

            var b = Matrix.Add(Matrix.Multiply(r, a),tExpanded);
            
            for (int i = 0; i < n; i++)
            {
                Correspondences.Add(new Correspondence
                {
                    LocalCoordinateSystem = new Vector3(Matrix.GetColumn(a, i).Select(j => (float)j).ToArray()),
                    ParentCoordinateSystem = new Vector3(Matrix.GetColumn(b, i).Select(j => (float)j).ToArray())
                });
            }
        }
    }
}
