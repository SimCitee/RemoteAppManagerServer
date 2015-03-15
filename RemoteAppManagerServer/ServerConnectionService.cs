using System;
using System.IO;
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
using Microsoft.Win32;
using Shell32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

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
        private List<ProcessPrototype> _processPrototypesList = null;
        private List<ProcessPrototype> _processesToStartList = null;
        private List<String> _imageStringList = null;

        public List<ProcessPrototype> ProcessPrototypesList { 
            get {
                if (_processPrototypesList == null) {
                    _processPrototypesList = new List<ProcessPrototype>();
                }

                return _processPrototypesList;
            } 
        }

        public List<ProcessPrototype> ProcessesToStartList {
            get {
                if (_processesToStartList == null) {
                    _processesToStartList = new List<ProcessPrototype>();
                }

                return _processesToStartList;
            }
        }

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

            foreach (IPAddress ip in ipHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }

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
                        Send(this.Socket, new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_END).Data);
                    }
                }
            }
        }

        private void InitSendProcessIcons(RemoteAppManager.Packets.Message message) {
            ServerUtils.DisplayMessage("Received request for icons");

            switch (message.Text) {
                case "kill":
                    RefreshProcessPrototypesList();
                    SendProcessIcon(0, ProcessPrototypesList);
                    break;
                case "start":
                    SendProcessIcon(0, ProcessesToStartList);
                    break;
            } 
        }

        private void RefreshProcessPrototypesList() {
            ProcessPrototypesList.Clear();

            //build filename list
            if (_processesCacheList != null) {
                foreach (Process process in _processesCacheList) {
                    String fileName = TryGetProcessFilename(process);

                    if (!String.IsNullOrEmpty(fileName)) {
                        ProcessPrototype proto = new ProcessPrototype(process.Id, fileName);
                        ProcessPrototypesList.Add(proto);
                    }
                }
            }
        }

        /// <summary>
        /// Send process icon to the client
        /// </summary>
        private void SendProcessIcon(int previousProcessID, List<ProcessPrototype> processesList) {
            if (processesList != null && processesList.Count > 0) {
                ProcessPrototype prototype = null;

                if (previousProcessID == 0) {
                    prototype = processesList.FirstOrDefault();
                }
                else {
                    prototype = processesList.SkipWhile(x => x.ProcessID != previousProcessID).Skip(1).FirstOrDefault();
                }

                if (prototype != null) {
                    Icon ico = Icon.ExtractAssociatedIcon(prototype.FileName);
                    if (ico != null) {
                        Bitmap bitmap = ico.ToBitmap();
                        RemoteAppManager.Packets.Message message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_IMAGE, Utils.BitmapToBase64String(bitmap) + ConnectionService.PROCESS_START_DELIMITER + prototype.ProcessID + ConnectionService.PROCESS_END_DELIMITER);

                        Send(this.Socket, message.Data, true);
                    }
                }
            }
        }

        private void TrySendProcess(Process process) {
            try {
                if (Socket.Connected && process.Id > 0) {

                    String data = process.Id + ";" + process.ProcessName;
                    RemoteAppManager.Packets.Message message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS, data);

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
                RemoteAppManager.Packets.Message message = null;

                ServerUtils.DisplayMessage("Received request to kill process [" + processID + "]");

                try {
                    Process process = Process.GetProcessById(processID);

                    if (process != null) {
                        ServerUtils.DisplayMessage("Killing process " + process.ProcessName);
                        process.Kill();

                        if (process.WaitForExit(2000) && process.HasExited) {
                            message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_KILL_SUCCESS, processID.ToString());
                        }
                        else {
                            message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_ERROR);
                        }
                    }
                }
                catch (Exception e) {
                    ServerUtils.DisplayMessage("Could not kill process [" + processID + "]");
                    Utils.Log(LogLevels.WARNING, e.ToString());
                    message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_ERROR);
                }

                Send(Socket, message.Data);
            }
        }

        public void RequestSearchProcess(string process) {
            String currentUser = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            String localMachine32 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            String localMachine64 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key;

            ProcessesToStartList.Clear();

            key = Registry.CurrentUser.OpenSubKey(currentUser);
            SearchSubKey(process, key);

            key = Registry.LocalMachine.OpenSubKey(localMachine32);
            SearchSubKey(process, key);

            key = Registry.LocalMachine.OpenSubKey(localMachine64);
            SearchSubKey(process, key);

            TrySendProcessToStart(ProcessesToStartList.FirstOrDefault());
        }

        private void SearchSubKey(String process, RegistryKey key)
        {
            RegistryKey subkey;
            String name;
            
            foreach (String keyName in key.GetSubKeyNames()) {
                try {
                    subkey = key.OpenSubKey(keyName);
                    name = subkey.GetValue("DisplayName").ToString();

                    if (name.IndexOf(process, StringComparison.OrdinalIgnoreCase) >= 0) {
                        foreach (String filePath in Directory.GetFiles(subkey.GetValue("InstallLocation").ToString(), "*.exe")) {
                            if (ProcessesToStartList.Where(x => x.FileName == filePath).Count() == 0) {
                                ProcessPrototype pts = new ProcessPrototype(ProcessesToStartList.Count(), filePath, name);
                                ProcessesToStartList.Add(pts);
                            }
                        }
                    }
                }
                catch (Exception e){
                    Utils.Log(LogLevels.ERROR, e.ToString());
                }
            }
        }

        private void TrySendProcessToStart(ProcessPrototype process)
        {
            try
            {
                if (Socket.Connected && process != null && process.ProcessID >= 0) {

                    String data = process.ProcessID + ";" + process.Name;
                    RemoteAppManager.Packets.Message message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_TO_START, data);

                    Send(Socket, message.Data, true);
                }
            }
            catch (Exception e)
            {
                Utils.Log(LogLevels.WARNING, "Process not sent " + process.ToString());
            }
        }

        public void SendProcessToStart(int previousProcessID) {
            if (ProcessesToStartList != null)
            {
                ProcessPrototype process = ProcessesToStartList.SkipWhile(x => x.ProcessID != previousProcessID).Skip(1).FirstOrDefault();

                if (process != null)
                {
                    TrySendProcessToStart(process);
                }
                else
                {
                    Send(this.Socket, new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_TO_START_END).Data);
                }
            }
        }

        public void StartProcess(int processID)
        {
            if (ProcessesToStartList != null)
            {
                Process myProcess;
                myProcess = Process.Start(ProcessesToStartList.Where(x => x.ProcessID == processID).Select(x => x.FileName).First());

                myProcess.WaitForInputIdle();
                System.Threading.Thread.Sleep (2000);

                PrintScreen();
            }
        }

        private void PrintScreen() {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            String image = Utils.BitmapToBase64String(printscreen);

            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            _imageStringList = Enumerable.Range(0, image.Length / 3000).Select(i => image.Substring(i * 3000, 3000)).ToList();

            SendProcessImage(_imageStringList.First());
        }

        private void SendProcessImage(String imageString) {
            RemoteAppManager.Packets.Message message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_IMAGE, imageString + ConnectionService.PROCESS_START_DELIMITER + "0" + ConnectionService.PROCESS_END_DELIMITER);

            Send(this.Socket, message.Data, true);
        }

        private void SendNextProcessImage(int previousImagePiece) {
            String imageString = _imageStringList.ElementAtOrDefault(previousImagePiece);

            previousImagePiece++;

            if (imageString != null) {
                RemoteAppManager.Packets.Message message = new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_IMAGE, imageString + ConnectionService.PROCESS_START_DELIMITER + previousImagePiece + ConnectionService.PROCESS_END_DELIMITER);

                Send(this.Socket, message.Data, true);
            }
            else {
                Send(this.Socket, new RemoteAppManager.Packets.Message(MessageTypes.RESPONSE_PROCESS_IMAGE_END).Data);
            }
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
        public void ServerConnectionService_MessageReceived(RemoteAppManager.Packets.Message message) {
            switch (message.Type) {
                case MessageTypes.REQUEST_PROCESSES:
                    InitSendProcess();
                    break;
                case MessageTypes.REQUEST_NEXT_PROCESS:
                    SendProcess(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_ICONS:
                    InitSendProcessIcons(message);
                    break;
                case MessageTypes.REQUEST_NEXT_ICON:
                    SendProcessIcon(message.GetIntegerValue, ProcessPrototypesList);
                    break;
                case MessageTypes.REQUEST_KILL_PROCESS:
                    RequestKillProcess(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_SEARCH_PROCESS:
                    RequestSearchProcess(message.Text);
                    break;
                case MessageTypes.REQUEST_NEXT_PROCESS_TO_START:
                    SendProcessToStart(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_START_PROCESS:
                    StartProcess(message.GetIntegerValue);
                    break;
                case MessageTypes.REQUEST_PROCESS_IMAGE_NEXT:
                    SendNextProcessImage(message.GetIntegerValue);
                    break;
            }
        }
        #endregion
    }
}
