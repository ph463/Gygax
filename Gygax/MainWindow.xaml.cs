using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GygaxCore;
using System.Windows.Forms;
using GygaxVisu.Controls;
using GygaxVisu.Visualizer;
using GygaxCore.Devices;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using Image = GygaxCore.Image;
using MenuItem = System.Windows.Controls.MenuItem;

namespace GygaxVisu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new ViewModel();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            StreamList.SelectionChanged += StreamListOnSelectionChanged;

            //_viewModel.Items.CollectionChanged += Stage.Items_CollectionChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!propertyChangedEventArgs.PropertyName.Equals("ShowCommon3DSpace"))
                return;

            if(_viewModel.ShowCommon3DSpace)
            {
                SelectedItemStage.Visibility = Visibility.Hidden;
                CommonStage.Visibility = Visibility.Visible;
                CommonStage.UpdateView();
            }
            else
            {
                SelectedItemStage.Visibility = Visibility.Visible;
                CommonStage.Visibility = Visibility.Hidden;
            }
        }

        private void StreamListOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            //Stage.MainGrid.Children.Add(((System.Windows.Controls.ListBox)sender).SelectedItems[0]);

            if(((System.Windows.Controls.ListBox)sender).SelectedItems.Count > 0)
                SelectedItemStage.MainGrid.DataContext = ((System.Windows.Controls.ListBox) sender).SelectedItems[0];

        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (var item in _viewModel.Items)
            {
                ((IStreamable)item).Close();
            }
        }

    }
}
