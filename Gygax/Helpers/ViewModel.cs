using GygaxCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using GygaxCore.DataStructures.DataStructures.Interfaces;
using GygaxVisu.Controls;

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

        private bool _useGlobalCamera = true;
        public bool UseGlobalCamera
        {
            get
            {
                return _useGlobalCamera;
            }
            set
            {
                _useGlobalCamera = value;
                NotifyPropertyChanged("UseGlobalCamera");
            }
        }

        private bool _showConsole = false;
        public bool ShowConsole
        {
            get
            {
                return _showConsole;
            }
            set
            {
                _showConsole = value;
                NotifyPropertyChanged("ShowConsole");
            }
        }

        private bool _showSelectedInMainStage = true;
        public bool ShowSelectedInMainStage
        {
            get
            {
                return _showSelectedInMainStage;
            }
            set
            {
                _showSelectedInMainStage = value;
                NotifyPropertyChanged("ShowSelectedInMainStage");
            }
        }


        public delegate void AddListElement(IProcessor p);
        public static AddListElement AddElementToList;

        public ViewModel()
        {
            Items = new ObservableCollection<IStreamable>();
            AddElementToList = addElementToList;

            PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case "UseGlobalCamera":
            //        Control3D. = ((Boolean) sender);
            //}
        }

        private void addElementToList(IProcessor processor)
        {
            Items.Add(processor);
        }

        public ObservableCollection<IStreamable> Items
        {
            get; set;
        }

        public void Clear()
        {
            ClearWorkspace.Invoke(this, EventArgs.Empty);


            foreach (var item in Items)
            {
                item.Close();
            }

            Items.Clear();

        }

        public event EventHandler ClearWorkspace;



    }
}
