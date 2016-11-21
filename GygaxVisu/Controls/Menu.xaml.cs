using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GygaxCore;
using GygaxCore.DataStructures;
using GygaxCore.Devices;
using GygaxCore.Ifc;
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

        private void OpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            string[] filters = new[]
            {
                "All Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;*.bmp*.avi;*.wmv;*.mp4;*.mpg;*.mkv;*.gyg;*.pcd;*.js;*.nvm;*.ifc;*.txt",
                "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;*.bmp",
                "Video Files|*.avi;*.wmv;*.mp4;*.mpg;*.mkv;*.gyg",
                "Pointcloud Files|*.pcd;*.js;*.nvm",
                "BIM Files|*.ifc",
                "Camera Position Files|*.txt"
            };

            var res = new OpenFileDialog
            {
                Filter = String.Join("|", filters)
            };

            if (res.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            switch (res.FileName.Split('.').Last().ToLower())
            {
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "tif":
                case "bmp":
                    _viewModel.Items.Add(new Image(res.FileName));
                    break;

                case "gyg":
                    _viewModel.Items.Add(new GygaxVideo(res.FileName));
                    break;

                case "avi":
                case "wmv":
                case "mp4":
                case "mpg":
                case "mkv":
                    _viewModel.Items.Add(new Video(res.FileName));
                    break;

                case "pcd":
                    _viewModel.Items.Add(new Pointcloud(res.FileName));
                    break;

                case "js":
                    _viewModel.Items.Add(new Potree(new Uri(res.FileName)));
                    break;

                case "nvm":
                    _viewModel.Items.Add(new VsfmReconstruction(res.FileName));
                    break;

                case "ifc":
                    _viewModel.Items.Add(new IfcViewerWrapper(res.FileName));
                    break;

                case "txt":
                    _viewModel.Items.Add(new SceneScannerPosition(res.FileName));
                    break;
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
                menuItem.Click += delegate
                {
                    if(!UsbCamera.IsCameraOpened(camera.i))
                        _viewModel.Items.Add(new UsbCamera(camera.i));
                };

                LocalCamera.Items.Add(menuItem);
            }

            LocalCamera.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = "All cameras";
            menuItem.Click += delegate
            {
                foreach (var camera in cameraList.Select((value, i) => new { i, value }))
                {
                    if (!UsbCamera.IsCameraOpened(camera.i))
                        _viewModel.Items.Add(new UsbCamera(camera.i));
                }
            };

            LocalCamera.Items.Add(menuItem);
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void ClearWorkspace_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Clear();
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

            //var res = new OpenFileDialog
            //{
            //    Filter = "NView Match|*.nvm;..."
            //};

            //if (!(res.ShowDialog() == DialogResult.OK))
            //{
            //    return;
            //}

            //var res2 = new OpenFileDialog
            //{
            //    Filter = "Ifc Files|*.ifc;..."
            //};

            //if (!(res2.ShowDialog() == DialogResult.OK))
            //{
            //    return;
            //}

            //FolderBrowserDialog res3 = new FolderBrowserDialog();

            //if (!(res3.ShowDialog() == DialogResult.OK))
            //{
            //    return;
            //}

            //Methods.CalculateOneTexture(res.FileName, res2.FileName, res3.SelectedPath + @"\");
            Methods.CalculateOneTexture(@"Z:\06. Data\Bridges\Philipp\Bridge 1\JustImages\sparse.nvm", @"Z:\06. Data\Bridges\Philipp\Bridge 1\IFC\Bridge1_v3.ifc", @"Z:\06. Data\Bridges\Philipp\Bridge 1\Textures\");
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
            var res = new OpenFileDialog
            {
                Filter = "Bin file|*.bin;..."
            };

            if (!(res.ShowDialog() == DialogResult.OK))
            {
                return;
            }

            var m = new Methods();
            m.DrawTexture(res.FileName);
        }

        private void ObjectExport_OnClick(object sender, RoutedEventArgs e)
        {
            Methods.ExportToObject();
        }
    }
}
