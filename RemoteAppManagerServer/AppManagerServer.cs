using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using RemoteAppManager;
using RemoteAppManager.Core;

namespace RemoteAppManagerServer
{
    class AppManagerServer : IAppManagerServer
    {

        #region Constants
        private const int MAX_CONNECTION = 1;

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        #endregion

        #region View properties
        private ConnectionService _connection;

        public ConnectionService Connection {
            get { return _connection; }
        }
        #endregion

        #region Class
        public AppManagerServer() {
            _connection = new ConnectionService();
            _connection.MessageReceived += new MessageReceivedEventHandler(_connectionService_MessageReceived);
        }
        #endregion

        public void Listen() {
            byte[] bytes = new Byte[1024];

            ServerUtils.DisplayMessage("Initializing server...");

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList.Last();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, ConnectionService.PORT);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {
                listener.Bind(localEndPoint);
                listener.Listen(MAX_CONNECTION);

                ServerUtils.DisplayMessage("Server listening");

                while (true) {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e) {
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            ServerUtils.DisplayMessage("Connection accepted from remote " + handler.RemoteEndPoint.ToString());

            Connection.State.WorkSocket = handler;
            Connection.BeginReceive(handler);
        }

        public void RequestProcesses() {
            Process[] processes = Process.GetProcesses();

            ServerUtils.DisplayMessage("Received request for processes");

            foreach (Process process in processes) {
                TrySendProcess(process);
            }
        }

        public void RequestKillProcess(int processID) {
            Message message = new Message();

            try {
                Process process = Process.GetProcessById(processID);

                if (process != null) {
                    process.Kill();

                    System.Threading.Thread.Sleep(1000);

                    if (process.HasExited) {
                        message.MessageType = MessageTypes.MESSAGE_KILL_SUCCESS;
                        message.Data = processID;
                    }
                    else {
                        message.MessageType = MessageTypes.MESSAGE_ERROR;
                    }
                }
            }
            catch (Exception e) {
                ServerUtils.DisplayMessage("Could not kill process [" + processID + "]");
                Utils.Log(Utils.LogLevels.WARNING, e.ToString());
                message.MessageType = MessageTypes.MESSAGE_ERROR;
            }

            Connection.Send(Connection.State.WorkSocket, message);
        }

        public void RequestStartProcess(string process) {
            throw new NotImplementedException();
        }

        private void TrySendProcess(Process process) {
            try {
                if (process.Id > 0) {
                    String fileName = process.MainModule.FileName;
                    String data = process.Id + ";" + process.ProcessName;

                    Message message = new Message(MessageTypes.MESSAGE_PROCESS, data);

                    Connection.Send(Connection.State.WorkSocket, message);

                    System.Threading.Thread.Sleep(1000);

                    Icon ico = Icon.ExtractAssociatedIcon(fileName);
                    if (ico != null) {
                        Bitmap bitmap = ico.ToBitmap();
                        bitmap.Tag = process.Id;
                        Connection.Send(Connection.State.WorkSocket, bitmap);
                    }
                }
            }
            catch (Exception e) {
                Utils.Log(Utils.LogLevels.WARNING, "Process not sent " + process.ToString());
            }
        }

        #region Sub events
        private void _connectionService_MessageReceived(Message message) {
            switch (message.MessageType) {
                case MessageTypes.MESSAGE_REQUEST_PROCESSES:
                    RequestProcesses();
                    break;
                case MessageTypes.MESSAGE_KILL_PROCESS:
                    int processID;

                    if (message.Data != null && Int32.TryParse(message.Data.ToString(), out processID)) {
                        RequestKillProcess(processID);
                    }
                    break;
            }
        }
        #endregion
    }
}
