using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Interop;
using Microsoft.TeamFoundation.MVVM;
using RemoteAppManager;
using RemoteAppManager.Core;
using RemoteAppManagerClient.MVVM;
using RemoteAppManagerClient.Prototype;
using RemoteAppManager.Packets;

namespace RemoteAppManagerClient
{
    class ClientConnectionService : ConnectionService, IClientConnexionService
    {
        public void Start(IPAddress ipAddress) {
            try {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, ConnectionService.APPLICATION_PORT);

                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), Socket);
            }
            catch (Exception e) {
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                if (Socket.Connected) {
                    Socket.EndConnect(ar);

                    OnConnectionStateChanged(ConnectionStatuses.CONNECTED);

                    Receive(Socket);
                }
            }
            catch (Exception e) {
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }
    }
}
