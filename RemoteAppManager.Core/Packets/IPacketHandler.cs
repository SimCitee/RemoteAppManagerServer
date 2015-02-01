using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteAppManager.Packets
{
    interface IPacketHandler
    {
        void Handle(byte[] packet, Socket handler);
    }
}
