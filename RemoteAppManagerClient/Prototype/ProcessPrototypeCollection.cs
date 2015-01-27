using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using RemoteAppManagerClient.Prototype;

namespace RemoteAppManagerClient
{
    class ProcessPrototypeCollection : ObservableCollection<ProcessPrototype>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (ProcessPrototype element in e.NewItems) {
                    element.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnMatrixElementModelPropertyChanged);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (ProcessPrototype element in e.OldItems) {
                    element.PropertyChanged += null;
                }
            }

            base.OnCollectionChanged(e);
        }

        private void OnMatrixElementModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            OnPropertyChanged(new PropertyChangedEventArgs(e.PropertyName));
        }
    }
}
