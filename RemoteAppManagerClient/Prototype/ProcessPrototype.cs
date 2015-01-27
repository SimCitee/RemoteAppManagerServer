using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using RemoteAppManagerClient.MVVM;

namespace RemoteAppManagerClient.Prototype
{
    class ProcessPrototype : ViewModelBase
    {
        #region Properties
        private int _ID;
        private String _name;
        private BitmapImage _icon;

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

        #region View properties
        public BitmapImage Icon {
            get { return _icon; }
        }
        #endregion

        #region Class
        public ProcessPrototype(int processID, String processName) {
            _ID = processID;
            _name = processName;
        }
        #endregion

        #region Methods
        public void AddIcon(BitmapImage icon) {
            if (icon != null) {
                _icon = icon;
            }
        }
        #endregion
    }
}
