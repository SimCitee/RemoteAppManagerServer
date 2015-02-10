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
using System.Timers;
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
        private Process[] _processesCacheList = null;
        List<ProcessIconPrototype> _processPrototypesList = null;
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
            this.Socket = listener.EndAccept(ar);

            ServerUtils.DisplayMessage("Connection accepted from remote " + Socket.RemoteEndPoint.ToString());

            Receive(Socket);
        }

        private void InitSendProcess() {
            ServerUtils.DisplayMessage("Received request for processes");
            _processesCacheList = Process.GetProcesses();

            Process process = _processesCacheList.FirstOrDefault();

            if (process != null) {
                TrySendProcess(process);
            }
        }

        /// <summary>
        /// Send processes to the client
        /// </summary>
        public void SendProcess(int previousProcessID) {
            if (_processesCacheList != null) {
                Process process = _processesCacheList.SkipWhile(x => x.Id != previousProcessID).Skip(1).FirstOrDefault();

                if (process != null) {
                    TrySendProcess(process);

                    if (process == _processesCacheList.Last()) {
                        Send(this.Socket, new Message(MessageTypes.RESPONSE_PROCESS_END).Data);
                    }
                }
            }
        }

        private void InitSendProcessIcons() {
            ServerUtils.DisplayMessage("Received request for icons");

            _processPrototypesList = new List<ProcessIconPrototype>();

            //build filename list
            if (_processesCacheList != null) {
                foreach (Process process in _processesCacheList) {
                    String fileName = TryGetProcessFilename(process);

                    if (!String.IsNullOrEmpty(fileName)) {
                        ProcessIconPrototype proto = new ProcessIconPrototype(process.Id, fileName);
                        _processPrototypesList.Add(proto);
                    }
                }

                if (_processPrototypesList.Count > 0) {
                    SendProcessIcon(0);
                }
            }
        }

        /// <summary>
        /// Send process icon to the client
        /// </summary>
        private void SendProcessIcon(int previousProcessID) {
            if (_processPrototypesList != null) {
                ProcessIconPrototype prototype = null;

                if (previousProcessID == 0) {
                    prototype = _processPrototypesList.FirstOrDefault();
                }
                else {
                    prototype = _processPrototypesList.SkipWhile(x => x.ProcessID != previousProcessID).Skip(1).FirstOrDefault();
                }

                if (prototype != null) {
                    Icon ico = Icon.ExtractAssociatedIcon(prototype.FileName);
                    if (ico != null) {
                        Bitmap bitmap = ico.ToBitmap();
                        Message message = new Message(MessageTypes.RESPONSE_IMAGE, Utils.BitmapToBase64String(bitmap) + ConnectionService.PROCESS_START_DELIMITER + prototype.ProcessID + ConnectionService.PROCESS_END_DELIMITER);

                        Send(this.Socket, message.Data, true);
                    }
                }
            }
        }

        private void TrySendProcess(Process process) {
            try {
                if (Socket.Connected && process.Id > 0) {

                    String data = process.Id + ";" + process.ProcessName;
                    Message message = new Message(MessageTypes.RESPONSE_PROCESS, data);

                    Send(Socket, message.Data, true);
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

        /// <summary>
        /// Try and kill the process requested by the client
        /// </summary>
        /// <param name="processID"></param>
        public void RequestKillProcess(int processID) {
            if (processID > 0) {
                Message message = null;

                ServerUtils.DisplayMessage("Received request to kill process [" + processID + "]");

                try {
                    Process process = Process.GetProcessById(processID);

                    if (process != null) {
                        ServerUtils.DisplayMessage("Killing process " + process.ProcessName);
                        process.Kill();

                        if (process.WaitForExit(2000) && process.HasExited) {
                            message = new Message(MessageTypes.RESPONSE_KILL_SUCCESS, processID.ToString());
                        }
                        else {
                            message = new Message(MessageTypes.RESPONSE_ERROR);
                        }
                    }
                }
                catch (Exception e) {
                    ServerUtils.DisplayMessage("Could not kill process [" + processID + "]");
                    Utils.Log(LogLevels.WARNING, e.ToString());
                    message = new Message(MessageTypes.RESPONSE_ERROR);
                }

                Send(Socket, message.Data);
            }
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

        /// <summary>
        /// Handle message received from the connection service
        /// </summary>
        /// <param name="message">Message received</param>
        public void ServerConnectionService_MessageReceived(Message message) {
            switch (message.Type) {
                case MessageTypes.REQUEST_PROCESSES:
                    InitSendProcess();
                    break;
                case MessageTypes.REQUEST_NEXT_PROCESS:
                    SendProcess(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_ICONS:
                    InitSendProcessIcons();
                    break;
                case MessageTypes.REQUEST_NEXT_ICON:
                    SendProcessIcon(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_KILL_PROCESS:
                    RequestKillProcess(message.GetIntegerValue);
                    break;
            }
        }
        #endregion
    }
}
