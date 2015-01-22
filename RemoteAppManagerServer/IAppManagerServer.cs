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
        void RequestProcesses();
        void RequestKillProcess(int processID);
        void RequestStartProcess(String process);
    }
}
