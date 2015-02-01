using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerServer
{
    class Program
    {
        ServerConnectionService _server;
        static void Main(string[] args)
        {
            ServerConnectionService server = new ServerConnectionService();
            server.Listen();
        }
    }
}
