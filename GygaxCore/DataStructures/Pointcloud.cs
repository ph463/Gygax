using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using PclWrapper;
using SharpDX;
using IImage = Emgu.CV.IImage;

namespace GygaxCore.DataStructures
{
    public class Pointcloud : PCD, IStreamable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _stop;

        private object _data;

        public object Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public string Filename;

        public ImageSource ImageSource { get; }

        public IImage CvSource { get; set; }

        public void Close()
        {
            _stop = true;
        }

        public Pointcloud()
        { }

        public Pointcloud(string filename)
        {
            Filename = filename;

            var thread = new Thread(LoadThreadFunction)
            {
                Name = "Pointcloud loader " + Path.GetFileName(filename)
            };
            thread.Start();
        }
        
        private void LoadThreadFunction()
        {
            PCD wrapper = new PCD();

            Points[] rawPoints = wrapper.LoadPointcloud(Filename);

            var points = new PointGeometry3D();
            var col = new Color4Collection();
            var ptPos = new Vector3Collection();
            var ptIdx = new IntCollection();
            var ptNormals = new Vector3Collection();

            var numberOfElements = rawPoints.Length;

            var additionalTurns = 0;

            foreach (var point in rawPoints)
            {
                ptIdx.Add(ptPos.Count);

                ptPos.Add(new Vector3(point.x, point.z, -point.y));
                col.Add(new Color4(new Color3(point.r / (float)255, point.g / (float)255, point.b / (float)255)));
                ptNormals.Add(new Vector3(0, 1, 0));
            }

            if ((rawPoints.Length / 3) * 3 != rawPoints.Length)
            {
                additionalTurns = ((rawPoints.Length / 3 + 1) * 3) - rawPoints.Length;
            }

            for (int i = 0; i < additionalTurns; i++)
            {
                ptIdx.Add(ptPos.Count);

                ptPos.Add(ptPos[ptPos.Count-1]);
                col.Add(col[col.Count - 1]);
                ptNormals.Add(ptNormals[ptNormals.Count - 1]);
            }

            points.Positions = ptPos;
            points.Indices = ptIdx;
            points.Colors = col;
            //points.Normals = ptNormals;

            Data = points;
        }

        public static PointGeometry3D ConvertToPointGeometry3D(Points[] points)
        {
            var geometry = new PointGeometry3D();
            var col = new Color4Collection();
            var ptPos = new Vector3Collection();
            var ptIdx = new IntCollection();
            var ptNormals = new Vector3Collection();

            var additionalTurns = 0;

            foreach (var point in points)
            {
                ptIdx.Add(ptPos.Count);

                ptPos.Add(new Vector3(point.x, point.y, point.z));
                col.Add(new Color4(new Color3(point.r / (float)255, point.g / (float)255, point.b / (float)255)));
                ptNormals.Add(new Vector3(0, 1, 0));
            }

            if ((points.Length / 3) * 3 != points.Length)
            {
                additionalTurns = ((points.Length / 3 + 1) * 3) - points.Length;
            }

            for (int i = 0; i < additionalTurns; i++)
            {
                ptIdx.Add(ptPos.Count);

                ptPos.Add(ptPos[ptPos.Count - 1]);
                col.Add(col[col.Count - 1]);
                ptNormals.Add(ptNormals[ptNormals.Count - 1]);
            }

            geometry.Positions = ptPos;
            geometry.Indices = ptIdx;
            geometry.Colors = col;

            return geometry;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
