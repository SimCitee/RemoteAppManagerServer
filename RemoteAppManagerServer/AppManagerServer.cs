using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace RemoteAppManagerServer
{
    class AppManagerServer : IAppManagerServer
    {

        private const int PORT = 9999;
        private ConnectionService _connection;

        public AppManagerServer()
        {
            _connection = new ConnectionService();
        }

        public void Listen()
        {
            byte[] bytes = new Byte[1024];

            IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList.First();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void RequestProcesses()
        {
            throw new NotImplementedException();
        }

        public void RequestKillProcess(int processID)
        {
            throw new NotImplementedException();
        }

        public void RequestStartProcess(string process)
        {
            throw new NotImplementedException();
        }
    }
}
