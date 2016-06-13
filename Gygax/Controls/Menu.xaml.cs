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
using Image = GygaxCore.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;
using GygaxVisu.Visualizer;

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
                Filter = "Video Files|*.avi;*.wmv;*.mp4;*.mpg;*.mkv;..."
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
                var vsfm = new VsfmReconstruction(res.FileName);
                //_viewModel.Items.Add(vsfm);

                //MyProcessor mp = new MyProcessor();
                //mp.Source = vsfm;
                //vsfm.PropertyChanged += mp.SourceUpdated;

                _viewModel.Items.Add(vsfm);
            }
        }

        private void Scene_OnClick(object sender, RoutedEventArgs e)
        {
            var res = new OpenFileDialog
            {
                Filter = "Scene Scanner Positions|*.txt;..."
            };

            if (res.ShowDialog() == DialogResult.OK)
            {
                var sceneReader = new SceneScannerPositionReader(res.FileName);
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
    }
}
