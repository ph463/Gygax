using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GygaxVisu.Visualizer;
using GygaxCore;
using GygaxCore.Interfaces;
using SharpDX.Direct3D9;

namespace GygaxVisu
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private bool _showCommon3DSpace = false;
        public bool ShowCommon3DSpace
        {
            get
            {
                return _showCommon3DSpace;
            }
            set
            {
                _showCommon3DSpace = value;
                NotifyPropertyChanged("ShowCommon3DSpace");
            }
        }

        public ViewModel()
        {
            Items = new ObservableCollection<IStreamable>();
        }

        public ObservableCollection<IStreamable> Items
        {
            get; set;
        }




    }
}
