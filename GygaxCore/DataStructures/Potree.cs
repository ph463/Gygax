using System;
using System.Threading;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;

namespace GygaxCore.DataStructures
{
    public class Potree : Pointcloud, IStreamable
    {
        public Potree(Uri file)
        {
            Filename = file.LocalPath;

            var thread = new Thread(LoadThreadFunction)
            {
                Name = "Potree loader " + file
            };
            thread.Start();
        }
        
        private void LoadThreadFunction()
        {
            var pt = new PotreeNode(new Uri(Filename));
            
            var points = new PointGeometry3D();
            var col = new Color4Collection();
            var ptPos = new Vector3Collection();
            var ptIdx = new IntCollection();

            var j = 0;

            foreach (var p in pt.Compilation.Points)
            {
                ptPos.Add(new Vector3(p.x, p.y, p.z));
                ptIdx.Add(j);
                col.Add(new Color4(p.r/255f, p.g / 255f, p.b / 255f, p.a / 255f));
                j++;
            }

            var additionalTurns = 0;

            if ((pt.Compilation.Points.Count / 3) * 3 != pt.Compilation.Points.Count)
            {
                additionalTurns = ((pt.Compilation.Points.Count / 3 + 1) * 3) - pt.Compilation.Points.Count;
            }

            for (int i = 0; i < additionalTurns; i++)
            {
                ptIdx.Add(ptPos.Count);

                ptPos.Add(ptPos[ptPos.Count - 1]);
                col.Add(col[col.Count - 1]);
            }

            points.Positions = ptPos;
            points.Indices = ptIdx;
            points.Colors = col;

            Data = points;
        }
    }
}
