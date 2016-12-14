using GygaxCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using GygaxCore.DataStructures;
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

        public ViewModel()
        {
            Items = new ObservableCollection<IStreamable>();
            Items.CollectionChanged += ItemsOnCollectionChanged;
        }
        

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var list = (ObservableCollection<IStreamable>)sender;

            if (notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add)
                return;

            var element = list.First(q => q.Equals(notifyCollectionChangedEventArgs.NewItems[0]));

            element.OnClosing += CloseListItem;
        }

        public void CloseListItem(IStreamable item)
        {
            item.OnClosing -= CloseListItem;
            Items.Remove(item);
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

        public void SaveAll()
        {
            foreach (var item in Items)
            {
                item.Save();
            }
        }

        public event EventHandler ClearWorkspace;



    }
}
