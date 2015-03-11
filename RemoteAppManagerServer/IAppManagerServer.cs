using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerServer
{
    interface IAppManagerServer
    {
        void Listen();
        void AcceptCallback(IAsyncResult ar);
        void SendProcess(int previousProcessID);
        void RequestKillProcess(int processID);
        void RequestSearchProcess(String process);
    }
}
