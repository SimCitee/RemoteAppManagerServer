using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerClient
{
    interface IAppManagerClient
    {
        void Start();
        void ConnectCallback(IAsyncResult ar);
        void RequestProcesses();
        void RequestKillProcess(int processID);
        void RequestStartProcess(String processName);
    }
}
