using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GygaxCore.Interfaces;

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

            _viewModel.PropertyChanged += SelectedItemStage.UpdateStageSource;

            StreamList.SelectionChanged += StreamListOnSelectionChanged;

            //_viewModel.Items.CollectionChanged += Stage.Items_CollectionChanged;

            _viewModel.PropertyChanged += ShowConsole;
            

            _viewModel.ClearWorkspace += SelectedItemStage.ViewModelOnClearWorkspace;
            _viewModel.ClearWorkspace += CommonStage.ViewModelOnClearWorkspace;
            _viewModel.ClearWorkspace += ViewModelOnClearWorkspace;


        }

        private void ViewModelOnClearWorkspace(object sender, EventArgs eventArgs)
        {
            _viewModel.ShowCommon3DSpace = false;


            foreach (var s in StreamList.Items)
            {

            }
        }


        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private void ShowConsole(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("ShowConsole"))
                return;

            //Open Console
            if (((ViewModel) sender).ShowConsole)
            {
                AllocConsole();
                //Console.WriteLine("test");
            }
            else
            {
                FreeConsole();
            }
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

            if (((System.Windows.Controls.ListBox) sender).SelectedItems.Count > 0
                && _viewModel.ShowSelectedInMainStage
                )
            {
                SelectedItemStage.MainGrid.DataContext = ((System.Windows.Controls.ListBox) sender).SelectedItems[0];
            }

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
