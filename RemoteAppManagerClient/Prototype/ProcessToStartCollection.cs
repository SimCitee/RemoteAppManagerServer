using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RemoteAppManagerClient.Prototype
{
    class ProcessToStartCollection : ObservableCollection<ProcessToStart>
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            /*if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ProcessToStart element in e.NewItems)
                {
                    element.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnMatrixElementModelPropertyChanged);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ProcessToStart element in e.OldItems)
                {
                    element.PropertyChanged += null;
                }
            }*/

            base.OnCollectionChanged(e);
        }

        private void OnMatrixElementModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(e.PropertyName));
        }
    }
}
