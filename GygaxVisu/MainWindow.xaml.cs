using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GygaxCore.Interfaces;
using NLog;

namespace GygaxVisu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ViewModel _viewModel;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new ViewModel();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            StreamList.SelectionChanged += StreamListOnSelectionChanged;

            //_viewModel.Items.CollectionChanged += Stage.Items_CollectionChanged;

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
            if(((System.Windows.Controls.ListBox)sender).SelectedItems.Count > 0)
                SelectedItemStage.MainGrid.DataContext = ((System.Windows.Controls.ListBox) sender).SelectedItems[0];
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_viewModel.Items.Count == 0)
                return;

            try
            {
                foreach (var item in _viewModel.Items)
                {
                    ((IStreamable) item).Close();
                }
            }
            catch (Exception)
            {
            }

        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LogLine.Height = new GridLength(40);
        }

        private void StreamList_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
