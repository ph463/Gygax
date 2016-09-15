using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HelixToolkit.Wpf.SharpDX;

namespace GygaxVisu.Controls
{
    /// <summary>
    /// Interaction logic for MainStage.xaml
    /// </summary>
    public partial class MainStage : UserControl
    {
        public MainStage()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }
        
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_updateStageSource)
            {
                SelectedItemControl.Content = DataContext;
            }
        }

        private bool _updateStageSource = true;

        public void UpdateStageSource(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("ShowSelectedInMainStage"))
                return;

            _updateStageSource = ((ViewModel)sender).ShowSelectedInMainStage;

        }

        public void ViewModelOnClearWorkspace(object sender, EventArgs eventArgs)
        {
            SelectedItemControl.Content = null;
        }

    }
}
