using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerClient.Prototype
{
    class IPAddressPrototype
    {
        private int _segment1;
        private int _segment2;
        private int _segment3;
        private int _segment4;

        public int Segment1 { 
            get { return _segment1;}
            set { _segment1 = value; }
        }

        public int Segment2 {
            get { return _segment2; }
            set { _segment2 = value; }
        }

        public int Segment3 {
            get { return _segment3; }
            set { _segment3 = value; }
        }

        public int Segment4 {
            get { return _segment4; }
            set { _segment4 = value; }
        }

        public bool IsAddressComplete{
            get {
                return (_segment1 > 0 && _segment1 < 255 &&
                    _segment2 > 0 && _segment2 < 255 &&
                    _segment3 > 0 && _segment3 < 255 && 
                    _segment4 > 0 && _segment4 < 255);
            }
        }

        public String GetIPAdress() {
            return _segment1 + "." + _segment2 + "." + _segment3 + "." + _segment4;
        }
    }
}
