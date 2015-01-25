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
using Microsoft.TeamFoundation.MVVM;
using RemoteAppManager;
using RemoteAppManager.Core;
using RemoteAppManagerClient.MVVM;
using RemoteAppManagerClient.Prototype;

namespace RemoteAppManagerClient
{
    class AppManagerClient : RemoteAppManagerClient.MVVM.ViewModelBase, IAppManagerClient
    {
        private static ManualResetEvent _connectDone = new ManualResetEvent(false);
        private static ManualResetEvent _sendDone = new ManualResetEvent(false);
        private static ManualResetEvent _receiveDone = new ManualResetEvent(false);

        private IPAddressPrototype _addressPrototype;
        private ProcessPrototypeCollection _processCollection;
        private ConnectionService _connection;
        private ConnectionStatuses _connectionStatus;

        #region Enums
        public enum ConnectionStatuses
        {
            CONNECTING = 1,
            CONNECTED = 2,
            DISCONNECTED = 3
        }
        #endregion

        #region Properties
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

        public ConnectionService Connection {
            get { return _connection; }
        }

        public bool IsReady {
            get {
                return (!Connection.IsConnected && this.ConnectionStatus == ConnectionStatuses.DISCONNECTED);
            }
        }

        public bool IsConnected {
            get {
                return (Connection.IsConnected && this.ConnectionStatus == ConnectionStatuses.CONNECTED);
            }
        }
        #endregion

        #region Class
        public AppManagerClient() {
            _connection = new ConnectionService();
            _connection.ConnectionStateChanged += new ConnectionStateChangedEventHandler(_connectionService_ConnectionStateChanged);
            _connection.MessageReceived += new MessageReceivedEventHandler(_connectionService_MessageReceived);
            _connectionStatus = ConnectionStatuses.DISCONNECTED;

            CreateConnectCommand();
            CreateRequestProcessesCommand();
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
            return true;
        }

        private void ConnectCommandExecute(Object param) {
            Start();
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
        #endregion

        public void Start() {
            try {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList.Last();
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, ConnectionService.PORT);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), client);
                Connection.State.WorkSocket = client;
                
                _connectDone.WaitOne();

                ConnectionStatus = ConnectionStatuses.CONNECTING;
            }
            catch (Exception e) {
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
        }

        public void ConnectCallback(IAsyncResult ar) {
            try {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                _connectDone.Set();
                ConnectionStatus = ConnectionStatuses.CONNECTED;
            }
            catch (Exception e) {
                ConnectionStatus = ConnectionStatuses.DISCONNECTED;
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
             
            NotifyPropertyChanged("IsConnected");
        }

        public void RequestProcesses() {
            Message message = new Message(MessageTypes.MESSAGE_REQUEST_PROCESSES);

            Connection.Send(Connection.State.WorkSocket, message);
        }

        public void RequestKillProcess(int processID) {
            throw new NotImplementedException();
        }

        public void RequestStartProcess(string processName) {
            throw new NotImplementedException();
        }

        public void AddProcess(Object messageData) {
            if (messageData != null && messageData.GetType() == typeof(String)) {
                String data = (String)messageData;

                String[] dataArray = data.Split(';');
                int processID;

                if (dataArray.Count() == 2 && Int32.TryParse(dataArray[0], out processID)) {

                    ProcessPrototype process = ProcessCollection.FirstOrDefault(x => x.ID == processID);
                        
                    if (process == null) {
                        process = new ProcessPrototype(processID, dataArray[1]);

                        Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(() =>
                            {
                                ProcessCollection.Add(process);
                            }));

                        Connection.BeginReceive(Connection.State.WorkSocket);
                    }
                }
            }
        }

        #region Sub events
        private void _connectionService_ConnectionStateChanged(object sender, EventArgs e) {
            NotifyPropertyChanged("IsReady");
            NotifyPropertyChanged("IsConnected");
        }

        private void _connectionService_MessageReceived(Message message) {
            switch (message.MessageType) {
                case MessageTypes.MESSAGE_PROCESS:
                    AddProcess(message.Data);
                    break;
                case MessageTypes.MESSAGE_CLOSE:
                    Connection.Close();
                    break;
            }
        }
        #endregion
    }
}
