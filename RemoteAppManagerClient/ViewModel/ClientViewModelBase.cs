using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAppManager.Packets;

namespace RemoteAppManagerClient.ViewModel
{
    abstract class ClientViewModelBase : RemoteAppManagerClient.MVVM.ViewModelBase
    {
        abstract protected void RequestProcesses();
        abstract protected void RequestKillProcess(int processID);
        abstract protected void RequestStartProcess(string processName);
        abstract protected void RequestIcons();
        abstract protected void AddProcess(Message message);
        abstract protected void RemoveProcess(Message message);
        abstract protected void AddProcessIcon(Message message);
    }
}
