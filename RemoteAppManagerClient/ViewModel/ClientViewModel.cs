using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.TeamFoundation.MVVM;
using RemoteAppManager;
using RemoteAppManager.Core;
using RemoteAppManager.Packets;
using RemoteAppManagerClient.Prototype;

namespace RemoteAppManagerClient.ViewModel
{
    class ClientViewModel : ClientViewModelBase
    {
        private ClientConnectionService _connection;
        private ConnectionStatuses _connectionStatus;
        private IPAddressPrototype _addressPrototype;
        private ProcessPrototypeCollection _processCollection;
        private ProcessPrototype _selectedPrototype;

        #region Properties
        public ProcessPrototype SelectedPrototype {
            get { return _selectedPrototype; }
            set {
                _selectedPrototype = value;
                NotifyPropertyChanged("SelectedPrototype");
            }
        }

        public ConnectionStatuses ConnectionStatus {
            get { return _connectionStatus; }
            set {
                _connectionStatus = value;
                NotifyPropertyChanged("ConnectionStatus");
                NotifyPropertyChanged("IsReady");
                NotifyPropertyChanged("IsConnected");
            }
        }
        #endregion

        #region View properties
        public ClientConnectionService Connection {
            get { return _connection; }
        }

        public IPAddressPrototype IPAddress {
            get {
                if (_addressPrototype == null) {
                    _addressPrototype = new IPAddressPrototype();
                }

                return _addressPrototype;
            }
        }

        public ProcessPrototypeCollection ProcessCollection {
            get {
                if (_processCollection == null) {
                    _processCollection = new ProcessPrototypeCollection();
                }

                return _processCollection;
            }
        }

        public bool IsReady {
            get {
                return (Connection.Socket != null && !Connection.Socket.Connected && this.ConnectionStatus == ConnectionStatuses.DISCONNECTED);
            }
        }

        public bool IsConnected {
            get {
                return (Connection.Socket != null && Connection.Socket.Connected && this.ConnectionStatus == ConnectionStatuses.CONNECTED);
            }
        }
        #endregion

        #region Class
        public ClientViewModel() {
            _connection = new ClientConnectionService();
            _connection.ConnectionStateChanged += new ConnectionStateChangedEventHandler(_connection_ConnectionStateChangedEventHandler);
            _connection.MessageReceived += new MessageReceivedEventHandler(_connection_MessageReceivedEventHandler); 

            CreateConnectCommand();
            CreateDisconnectCommand();
            CreateRequestProcessesCommand();
            CreateKillProcessCommand();
        }
        #endregion

        #region Commands
        private void CreateConnectCommand() {
            ConnectCommand = new RelayCommand(ConnectCommandExecute, CanExecuteConnectCommand);
        }

        public ICommand ConnectCommand {
            get;
            internal set;
        }

        private bool CanExecuteConnectCommand(Object param) {
            return !IsConnected;
        }

        private void ConnectCommandExecute(Object param) {
            IPAddress ipAddress = null;
            System.Net.IPAddress.TryParse(this.IPAddress.GetIPAdress(), out ipAddress);

            if (ipAddress != null) {
                Connection.Start(ipAddress);
            }
        }

        private void CreateDisconnectCommand() {
            DisconnectCommand = new RelayCommand(DisconnectCommandExecute, CanExecuteDisconnectCommand);
        }

        public ICommand DisconnectCommand {
            get;
            internal set;
        }

        private bool CanExecuteDisconnectCommand(Object param) {
            return IsConnected;
        }

        private void DisconnectCommandExecute(Object param) {
            Connection.Disconnect();

            SelectedPrototype = null;
            ProcessCollection.Clear();

            RefreshProperties();
        }

        private void CreateRequestProcessesCommand() {
            RequestProcessesCommand = new RelayCommand(RequestProcessesCommandExecute, CanExecuteRequestProcessesCommand);
        }

        public ICommand RequestProcessesCommand {
            get;
            internal set;
        }

        private bool CanExecuteRequestProcessesCommand(Object param) {
            return IsConnected;
        }

        private void RequestProcessesCommandExecute(Object param) {
            RequestProcesses();
        }

        private void CreateKillProcessCommand() {
            KillProcessCommand = new RelayCommand(KillProcessCommandExecute, CanExecuteKillProcessCommand);
        }

        public ICommand KillProcessCommand {
            get;
            internal set;
        }

        private bool CanExecuteKillProcessCommand(Object param) {
            return IsConnected && _selectedPrototype != null && _selectedPrototype.ID > 0;
        }

        private void KillProcessCommandExecute(Object param) {
            //RequestKillProcess(_selectedPrototype.ID);
        }
        #endregion

        #region Sub-model events
        private void _connection_ConnectionStateChangedEventHandler(ConnectionStatuses status) {
            ConnectionStatus = status;
            RefreshProperties();
        }

        private void _connection_MessageReceivedEventHandler(Message message) {
            switch (message.Type) {
                case MessageTypes.MESSAGE_REQUEST_PROCESSES_END:
                    RequestIcons();
                    break;
                case MessageTypes.MESSAGE_IMAGE:
                    AddProcessIcon(message);
                    break;
                case MessageTypes.MESSAGE_PROCESS:
                    AddProcess(message);
                    break;
            //    case MessageTypes.MESSAGE_KILL_SUCCESS:
            //        RemoveProcess(message.Data);
            //        break;
            //    case MessageTypes.MESSAGE_CLOSE:
            //        Connection.Close();
            //        break;
            }
        }
        #endregion

        #region Methods
        protected override void RequestProcesses() {
            Message message = new Message(MessageTypes.MESSAGE_REQUEST_PROCESSES);
            Connection.Send(Connection.Socket, message.Data);
        }

        protected override void RequestKillProcess(int processID) {
            Message message = new Message(MessageTypes.MESSAGE_KILL_PROCESS, processID.ToString());
            Connection.Send(Connection.Socket, message.Data);
        }

        protected override void RequestStartProcess(string processName) {
            throw new NotImplementedException();
        }

        protected override void AddProcess(Message message) {
            if (message != null) {
                String[] dataArray = message.Text.Split(';');
                int processID;

                if (dataArray.Count() >= 2 && Int32.TryParse(dataArray[0], out processID)) {

                    ProcessPrototype process = ProcessCollection.FirstOrDefault(x => x.ID == processID);

                    if (process == null) {
                        process = new ProcessPrototype(processID, dataArray[1]);

                        Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(() =>
                            {
                                ProcessCollection.Add(process);
                            }));
                    }
                }
            }
        }

        protected override void RemoveProcess(Message message) {
            //if (messageData != null && messageData.GetType() == typeof(String)) {
            //    String data = (String)messageData;
            //    int processID;

            //    if (Int32.TryParse(data, out processID)) {

            //        ProcessPrototype process = ProcessCollection.FirstOrDefault(x => x.ID == processID);

            //        if (process != null) {
            //            Application.Current.Dispatcher.BeginInvoke(
            //                DispatcherPriority.Background,
            //                new Action(() =>
            //                {
            //                    ProcessCollection.Remove(process);
            //                }));
            //        }
            //    }
            //}
        }

        protected override void RequestIcons() {
            Message message = new Message(MessageTypes.MESSAGE_REQUEST_ICONS);
            Connection.Send(Connection.Socket, message.Data);
        }

        protected override void AddProcessIcon(Message message) {
            if (message != null) {
                int processID;

                if (Int32.TryParse(Utils.GetTextBetween(message.Text, "<ID>", "</ID>"), out processID)) {
                    String imageText = message.Text.Substring(0, message.Text.IndexOf("<ID>"));

                    Bitmap bitmap = Utils.Base64StringToBitmap(imageText);

                    if (bitmap != null) {
                        ProcessPrototype prototype = _processCollection.FirstOrDefault(x => x.ID == processID);

                        if (prototype != null) {
                            ImageSource imageSource = Utils.BitmapToImageSource(bitmap);
                            bitmap.Dispose();

                            prototype.AddIcon(imageSource);
                        }
                    }
                }
            }
        }

        public void RefreshProperties() {
            NotifyPropertyChanged("IsReady");
            NotifyPropertyChanged("IsConnected");
        }
        #endregion
    }
}
