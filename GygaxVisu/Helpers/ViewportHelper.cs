using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using MathNet.Numerics.Interpolation;
using SharpDX;
using HelixToolkit.Wpf.SharpDX;

namespace GygaxVisu.Helpers
{
    public static class ViewportHelper
    {
        public static MeshGeometryModel3D GetSphere(Vector3 position, double radius = 0.1, PhongMaterial color = null)
        {
            MeshBuilder mb = new MeshBuilder();
            MeshGeometryModel3D model = new MeshGeometryModel3D();

            mb.AddSphere(position, radius);

            var geometry = mb.ToMeshGeometry3D();

            model.Geometry = geometry;

            if (color == null)
                color = PhongMaterials.Yellow;

            model.Material = color;

            return model;
        }

        public static void AddSphere(Vector3 position, double radius, PhongMaterial color, Viewport3DX viewport)
        {
            var m = GetSphere(position, radius, color);

            m.Attach(viewport.RenderHost);

            viewport.Items.Add(m);
        }

        // Existing ZoomExtents seems to be buggy, so here is the slightly adapted version
        internal static void ZoomExtents(Viewport3DX viewport)
        {
            var limit = new BoundingBox();
            var initial = true;

            foreach (var element in viewport.Items)
            {
                var model = element as IBoundable;
                if (model != null)
                {
                    if (model.Visibility != Visibility.Collapsed)
                    {
                        if (initial)
                        {
                            limit = GetBounds(model);

                            initial = false;
                        }
                        else
                        {
                            var value2 = GetBounds(model);
                            
                            Vector3.Min(ref limit.Minimum, ref value2.Minimum, out limit.Minimum);
                            Vector3.Max(ref limit.Maximum, ref value2.Maximum, out limit.Maximum);
                        }
                    }
                }
            }

            var bounds = new Rect3D();

            try
            {
                var size = limit.Maximum - limit.Minimum;
                var location = limit.Minimum.ToPoint3D();

                if (size.X < 0)
                {
                    size.X *= -1;
                    location.X -= size.X;
                }

                if (size.Y < 0)
                {
                    size.Y *= -1;
                    location.Y -= size.Y;
                }

                if (size.Z < 0)
                {
                    size.Z *= -1;
                    location.Z -= size.Z;
                }
                
                bounds = new Rect3D(location, size.ToSize3D());
            }
            catch (Exception)
            {
                return;
            }
            
            var diagonal = new Vector3D(bounds.SizeX, bounds.SizeY, bounds.SizeZ);

            if (bounds.IsEmpty || diagonal.LengthSquared.Equals(0))
            {
                return;
            }

            var center = bounds.Location + (diagonal * 0.5);
            double radius = diagonal.Length * 0.5;

            var camera = viewport.Camera;
            var pcam = camera as HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
            if (pcam != null)
            {
                double disth = radius / Math.Tan(0.5 * pcam.FieldOfView * Math.PI / 180);
                double vfov = pcam.FieldOfView / viewport.ActualWidth * viewport.ActualHeight;
                double distv = radius / Math.Tan(0.5 * vfov * Math.PI / 180);

                double dist = Math.Max(disth, distv);
                var dir = pcam.LookDirection;
                dir.Normalize();
                pcam.LookAt(center, dir * dist, 0);
            }
        }

        private static BoundingBox GetBounds(IBoundable model)
        {
            BoundingBox bounds = new BoundingBox();

            var m = model as MeshGeometryModel3D;
            if (m != null)
            {
                bounds = BoundingBox.FromPoints(m.Geometry.Positions.ToArray());

                var min = new Point3D(bounds.Minimum.X, bounds.Minimum.Y, bounds.Minimum.Z);
                var max = new Point3D(bounds.Maximum.X, bounds.Maximum.Y, bounds.Maximum.Z);

                bounds = new BoundingBox(m.Transform.Transform(min).ToVector3(), m.Transform.Transform(max).ToVector3());
            }

            var n = model as PointGeometryModel3D;
            if (n != null)
            {
                var min = new Point3D(
                    n.Geometry.Positions.Min(e => e.X),
                    n.Geometry.Positions.Min(e => e.Y),
                    n.Geometry.Positions.Min(e => e.Z)
                    );
                var max = new Point3D(
                    n.Geometry.Positions.Max(e => e.X),
                    n.Geometry.Positions.Max(e => e.Y),
                    n.Geometry.Positions.Max(e => e.Z)
                    );

                bounds = new BoundingBox(n.Transform.Transform(min).ToVector3(), n.Transform.Transform(max).ToVector3());
            }

            var o = model as LineGeometryModel3D;
            if (o != null)
            {
                bounds = BoundingBox.FromPoints(o.Geometry.Positions.ToArray());

                var min = new Point3D(bounds.Minimum.X, bounds.Minimum.Y, bounds.Minimum.Z);
                var max = new Point3D(bounds.Maximum.X, bounds.Maximum.Y, bounds.Maximum.Z);

                bounds = new BoundingBox(o.Transform.Transform(min).ToVector3(), o.Transform.Transform(max).ToVector3());
            }

            return bounds;
        }
    }
}
