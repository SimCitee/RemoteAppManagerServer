using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAppManagerClient.MVVM;

namespace RemoteAppManagerClient.Prototype
{
    class ProcessPrototype : ViewModelBase
    {
        #region Properties
        private int _ID;
        private String _name;

        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                NotifyPropertyChanged("ID");
            }
        }

        public String Name {
            get { return _name; }
            set {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }
        #endregion

        #region Class
        public ProcessPrototype(int processID, String processName) {
            _ID = processID;
            _name = processName;
        }
        #endregion
    }
}
