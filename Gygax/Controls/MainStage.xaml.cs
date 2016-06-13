using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
            SelectedItemControl.Content = DataContext;
        }
    }
}
