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
using RemoteAppManagerServer.Prototype;
using RemoteAppManager.Packets;

namespace RemoteAppManagerServer
{
    class ServerConnectionService : ConnectionService, IAppManagerServer
    {

        #region Constants
        private const int MAX_CONNECTION = 1;
        #endregion

        #region Properties
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        #endregion

        #region Class
        public ServerConnectionService() {
            ConnectionStateChanged += new ConnectionStateChangedEventHandler(ServerConnectionService_ConnectionStateChanged);
            MessageReceived += new MessageReceivedEventHandler(ServerConnectionService_MessageReceived); 
        }
        #endregion

        public void Listen() {
            ServerUtils.DisplayMessage("Initializing server...");

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList.Last();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, ConnectionService.APPLICATION_PORT);

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
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket = listener.EndAccept(ar);

            ServerUtils.DisplayMessage("Connection accepted from remote " + Socket.RemoteEndPoint.ToString());

            Receive(Socket);
        }

        public void RequestProcesses() {
            Process[] processes = Process.GetProcesses();
            ServerUtils.DisplayMessage("Received request for processes");

            foreach (Process process in processes) {
                TrySendProcess(process);
            }

            Send(this.Socket, new Message(MessageTypes.MESSAGE_REQUEST_PROCESSES_END).Data);
        }

        public void RequestIcons() {
            Process[] processes = Process.GetProcesses();
            List<ProcessIconPrototype> processPrototypes = new List<ProcessIconPrototype>();
            ServerUtils.DisplayMessage("Received request for icons");

            foreach (Process process in processes) {
                String fileName = TryGetProcessFilename(process);

                if (!String.IsNullOrEmpty(fileName)) {
                    ProcessIconPrototype proto = new ProcessIconPrototype(process.Id, fileName);
                    processPrototypes.Add(proto);
                }
            }

            foreach (ProcessIconPrototype prototype in processPrototypes) {
                Icon ico = Icon.ExtractAssociatedIcon(prototype.FileName);
                if (ico != null) {
                    Bitmap bitmap = ico.ToBitmap();
                    Message message = new Message(MessageTypes.MESSAGE_IMAGE, Utils.BitmapToBase64String(bitmap) + ConnectionService.PROCESS_START_DELIMITER + prototype.ProcessID + ConnectionService.PROCESS_END_DELIMITER);

                    Send(this.Socket, message.Data);

                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        private void TrySendProcess(Process process) {
            try {
                if (process.Id > 0) {
                    
                    String data = process.Id + ";" + process.ProcessName;
                    Message message = new Message(MessageTypes.MESSAGE_PROCESS, data);

                    Send(Socket, message.Data);

                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception e) {
                Utils.Log(LogLevels.WARNING, "Process not sent " + process.ToString());
            }
        }

        private String TryGetProcessFilename(Process process) {
            try {
                return process.MainModule.FileName;
            }
            catch (Exception e) {
                return String.Empty;
            }
        }

        public void RequestKillProcess(int processID) {
            Message message = null;

            ServerUtils.DisplayMessage("Received request to kill process [" + processID + "]");

            try {
                Process process = Process.GetProcessById(processID);

                if (process != null) {
                    ServerUtils.DisplayMessage("Killing process " + process.ProcessName);
                    process.Kill();

                    if (process.WaitForExit(2000) && process.HasExited) {
                        message = new Message(MessageTypes.MESSAGE_KILL_SUCCESS, processID.ToString());
                    }
                    else {
                        message = new Message(MessageTypes.MESSAGE_ERROR);
                    }
                }
            }
            catch (Exception e) {
                ServerUtils.DisplayMessage("Could not kill process [" + processID + "]");
                Utils.Log(LogLevels.WARNING, e.ToString());
                message = new Message(MessageTypes.MESSAGE_ERROR);
            }

            Send(Socket, message.Data);
        }

        public void RequestStartProcess(string process) {
            throw new NotImplementedException();
        }    

        #region Events
        private void ServerConnectionService_ConnectionStateChanged(ConnectionStatuses status) {
            switch (status) {
                case ConnectionStatuses.DISCONNECTED:
                    ServerUtils.DisplayMessage("Lost connection to remote...");
                    break;
            }
        }

        private void ServerConnectionService_MessageReceived(Message message) {
            switch (message.Type) {
                case MessageTypes.MESSAGE_REQUEST_PROCESSES:
                    RequestProcesses();
                    break;
                case MessageTypes.MESSAGE_REQUEST_ICONS:
                    RequestIcons();
                    break;
                case MessageTypes.MESSAGE_KILL_PROCESS:
                    int processID;

                    if (message.Data != null && Int32.TryParse(message.Text, out processID)) {
                        RequestKillProcess(processID);
                    }
                    break;
            }
        }
        #endregion
    }
}
