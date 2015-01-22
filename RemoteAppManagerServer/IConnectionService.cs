using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteAppManagerServer
{
    interface IConnectionService
    {
        #region Methods
        void ReadCallback(IAsyncResult ar);
        void Send(Socket handler, Message msg);
        void SendCallback(IAsyncResult ar);
        #endregion
    }
}
