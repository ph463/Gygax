using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GygaxVisu.Method;

namespace GenerateAllTextures
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            while (true)
            {
                var baseDir = @"Z:\06. Data\Bridges\Philipp";
                var subDir = @"\GainCompensation\GainCompensation";

                var dirs = Directory.GetDirectories(baseDir, "Bridge*");

                foreach (var d in dirs)
                {
                    //var bridgeFolder = "Bridge 1";
                    //var subDir = @"\GainCompensation\GainCompensation";
                    ////var subDir = "";

                    //string ifcFile = baseDir + @"\" + bridgeFolder + @"\IFC\Bridge1_v3.ifc";
                    //string textureDirectory = baseDir + @"\" + bridgeFolder + @"\Textures\";
                    
                    string ifcFile = d + @"\IFC\Bridge1_v3.ifc";
                    string textureDirectory = d + @"\Textures\";

                    var f = Directory.GetDirectories(d + @"\Sorted\");

                    foreach (var s in f)
                    {
                        string mainFile = s + subDir + @"\sparse.nvm";

                        if (!File.Exists(mainFile) || !File.Exists(s + @"\ifcElement.txt"))
                            continue;

                        Console.WriteLine("Starting with " + mainFile);

                        string[] elementName =
                            File.ReadAllLines(s + @"\ifcElement.txt");

                        Methods.CalculateTexture(mainFile, ifcFile, textureDirectory, elementName);
                    }
                }


                Thread.Sleep(300000);
            }
        }
    }
}
