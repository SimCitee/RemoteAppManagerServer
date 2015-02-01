using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteAppManager
{
    interface IConnectionService
    {
        #region Methods
        void Receive(Socket handler);
        void Send(Socket handler, byte[] data);
        void Disconnect();
        #endregion
    }
}
