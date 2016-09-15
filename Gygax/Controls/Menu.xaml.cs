using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using GygaxCore.DataStructures;
using GygaxCore.Devices;
using GygaxCore.Ifc;
using GygaxVisu.Helpers;
using GygaxVisu.Method;
using Image = GygaxCore.DataStructures.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;
using GygaxVisu.Visualizer;
using HelixToolkit.Wpf.SharpDX;
using PclWrapper;
using SharpDX;

namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        private ViewModel _viewModel {get { return (ViewModel)this.DataContext; } }

        public Menu()
        {
            InitializeComponent();
        }

        private void NewImage_OnClick(object sender, RoutedEventArgs e)
        {
            //Create a new instance of openFileDialog
            var res = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;*.bmp;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                _viewModel.Items.Add(new Image(res.FileName));
            }
        }

        private void VideoFromFile_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _viewModel.Items.Add(new VideoFromFolder { Folder = new Uri(fbd.SelectedPath) });
            }
        }

        private void FrameworkElement_OnInitialized(object sender, EventArgs e)
        {
            List<string> cameraList = UsbCamera.GetDevices();

            if (cameraList.Count > 0) LocalCamera.IsEnabled = true;

            MenuItem menuItem;

            foreach (var camera in cameraList.Select((value, i) => new { i, value }))
            {
                menuItem = new MenuItem();
                menuItem.Header = camera.value;
                menuItem.Click += delegate { _viewModel.Items.Add(new UsbCamera(camera.i)); };

                LocalCamera.Items.Add(menuItem);
            }

            LocalCamera.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = "All cameras";
            menuItem.Click += delegate
            {
                foreach (var camera in cameraList.Select((value, i) => new { i, value }))
                {
                    _viewModel.Items.Add(new UsbCamera(camera.i));
                }
            };

            LocalCamera.Items.Add(menuItem);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //Create a new instance of openFileDialog
            var res = new OpenFileDialog
            {
                Filter = "Video Files|*.avi;*.wmv;*.mp4;*.mpg;*.mkv;*.mts;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var video = new Video(res.FileName);
                _viewModel.Items.Add(video);

                //video.PropertyChanged += Stage.VOnPropertyChanged;
            }
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void PointCloud_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "Pointcloud Files|*.pcd;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                Pointcloud pcl = new Pointcloud(res.FileName);
                _viewModel.Items.Add(pcl);
            }
        }

        private void Potree_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "Potree Json File|*.js;..."
            };

            if (res.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var ptr = new PotreeLoader(new Uri(res.FileName));
            _viewModel.Items.Add(ptr);
        }

        private void Vsfm_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "NView Match|*.nvm;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var vsfm = VsfmReconstruction.OpenMultiple(res.FileName);

                foreach (var reconstruction in vsfm.Where(x => ((NViewMatch)x.Data).Transform != Matrix3D.Identity))
                {
                    _viewModel.Items.Add(reconstruction);
                }
                
            }
        }

        private void VsfmInclImages_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "NView Match|*.nvm;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var vsfm = VsfmReconstruction.OpenMultiple(res.FileName);
                
                foreach (var reconstruction in vsfm)
                {
                    _viewModel.Items.Add(reconstruction);

                    foreach (var image in ((NViewMatch)reconstruction.Data).CameraPositions)
                    {
                        _viewModel.Items.Add(new Image(image.File));
                    }
                }

            }
        }

        private void Correspondences_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "CSV file|*.csv;..."
            };

            if (res.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var cs = new CoordinateSystem();


            cs.LoadCorrespondences(res.FileName);

            cs.Transformation = CoordinateSystem.OpenTransformation(new Uri(Path.GetDirectoryName(res.FileName) + @"\sparse.tfm"));

            if (!Path.GetFileName(res.FileName).Equals("Correspondences.csv") &&
                Path.GetFileName(res.FileName).Split('.').Length == 3)
            {
                cs.Transformation = CoordinateSystem.OpenTransformation(new Uri(Path.GetDirectoryName(res.FileName) + @"\sparse."+ Path.GetFileName(res.FileName).Split('.')[1] + ".tfm"));
            }
            
            var str = new Streamable()
            {
                Data = cs
            };
            
            _viewModel.Items.Add(str);
        }

        private void Scene_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "Scene Scanner Positions|*.txt;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var sceneReader = new SceneScannerPositionLoader(res.FileName);
                _viewModel.Items.Add(sceneReader);

                //_viewModel.Items.Add();
            }
        }

        private void OpenIfcFile_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "Ifc Files|*.ifc;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var ifcModel = new IfcViewerWrapper();
                ifcModel.ParseIfcFile(res.FileName);

                _viewModel.Items.Add(ifcModel);

                //ifcTree.Items.Add(ifcModel.Tree);
            }
        }

        private void Correction_OnClick(object sender, RoutedEventArgs e)
        {
            var cs = new CoordinateSystem();

            cs.LoadCorrespondences(@"C:\Users\Philipp\Desktop\Correspondences.csv");
            
            //cs.AddTestelements();
            cs.CalculateHomography();

            //cs.SaveTransform(@"C:\Users\Philipp\Desktop\Transform.csv");
        }

        private void ColumnIcp_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.Icp(true);
        }

        private void ColumnNoIcp_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.Icp(false);
        }

        private void CalculateTexture_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.CalculateTexture();
        }

        private void GenerateAllTextures_OnClick(object sender, RoutedEventArgs e)
        {
            //var m = new Methods();
            //m.GenerateAllTextures();

            var res = new OpenFileDialog
            {
                Filter = "NView Match|*.nvm;..."
            };

            if (!(res.ShowDialog() == DialogResult.OK))
            {
                return;
            }

            var res2 = new OpenFileDialog
            {
                Filter = "Ifc Files|*.ifc;..."
            };

            if (!(res2.ShowDialog() == DialogResult.OK))
            {
                return;
            }

            FolderBrowserDialog res3 = new FolderBrowserDialog();

            if (!(res3.ShowDialog() == DialogResult.OK))
            {
                return;
            }

            Methods.CalculateOneTexture(res.FileName, res2.FileName, res3.SelectedPath + @"\");
        }

        private void BuildMasks_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.CalculateTexture(false, true);
        }

        private void ExtractImagePoints_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.ExtractFromImageCoordinates();
        }

        private void DrawTextures_OnClick(object sender, RoutedEventArgs e)
        {
            var m = new Methods();
            m.DrawTexture();
        }

        private void ClearWorkspace_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Clear();
        }
    }
}
