using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteAppManager
{
    public class StateObject
    {
        public const int BUFFER_SIZE = 1024;

        private Socket _socket = null;
        private byte[] _buffer = new byte[BUFFER_SIZE];
        private StringBuilder _stringBuilder = new StringBuilder();

        public Socket WorkSocket {
            get { return _socket; }
            set { _socket = value; }
        }

        public byte[] Buffer {
            get { return _buffer; }
        }

        public StringBuilder Builder {
            get { return _stringBuilder; }
        }

        public bool IsSocketConnected() {
            return (_socket != null && ((_socket.Poll(1000, SelectMode.SelectRead) && _socket.Available == 0) || _socket.Connected));
        }
    }
}
