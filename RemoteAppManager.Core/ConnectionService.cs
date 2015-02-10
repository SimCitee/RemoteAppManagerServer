using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Drawing;
using RemoteAppManager.Core;
using RemoteAppManager.Packets;

namespace RemoteAppManager
{
    #region Enums
    public enum ConnectionStatuses
    {
        CONNECTING = 1,
        CONNECTED = 2,
        DISCONNECTED = 3
    }
    #endregion

    public delegate void ConnectionStateChangedEventHandler(ConnectionStatuses status);
    public delegate void MessageReceivedEventHandler(Message message);

    public class ConnectionService : IConnectionService
    {
        public event ConnectionStateChangedEventHandler ConnectionStateChanged;
        public event MessageReceivedEventHandler MessageReceived;

        private Socket _socket;
        private byte[] _buffer = new byte[PacketStructure.BUFFER_SIZE];
        private bool _isWaitingResponse;
        private object _propertyLock = new object();
        private MessageTypes _responseType;

        #region Constants
        public const int APPLICATION_PORT = 9999;

        public const String PROCESS_START_DELIMITER = "<ID>";
        public const String PROCESS_END_DELIMITER = "</ID>";
        #endregion

        #region Properties
        public Socket Socket {
            get { return _socket; }
            set { _socket = value; }
        }

        public bool IsWaitingResponse {
            get { return _isWaitingResponse; }
            set {
                lock (_propertyLock) {
                    _isWaitingResponse = value;
                }
            }
        }

        public MessageTypes ResponseType {
            get { return _responseType; }
            set { _responseType = value; }
        }
        #endregion

        #region View properties
        public byte[] Buffer {
            get { return _buffer; }
        }
        #endregion

        #region Events
        protected virtual void OnConnectionStateChanged(ConnectionStatuses status) {
            if (ConnectionStateChanged != null) {
                ConnectionStateChanged(status);
            }
        }

        protected virtual void OnMessageReceived(Message message) {
            if (MessageReceived != null) {
                MessageReceived(message);
            }
        }
        #endregion

        #region Methods
        public void Receive(Socket handler) {
            handler.BeginReceive(_buffer, 0, PacketStructure.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), handler);
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                Socket clientSocket = ar.AsyncState as Socket;
                int bufferSize = clientSocket.EndReceive(ar);
                byte[] packet = new byte[bufferSize];
                Array.Copy(_buffer, packet, packet.Length);

                Handle(packet, clientSocket);

                _buffer = new byte[PacketStructure.BUFFER_SIZE];
                clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, clientSocket);
            }
            catch (Exception e) {
                OnConnectionStateChanged(ConnectionStatuses.DISCONNECTED);
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }

        public void Send(Socket handler, Message message) {
            Send(handler, message.Data);
        }

        public void Send(Socket handler, byte[] data) {
            Send(handler, data, false);
        }

        public void Send(Socket handler, byte[] data, bool awaitResponse) {
            try {
                IsWaitingResponse = awaitResponse;

                if (IsWaitingResponse) {
                    ResponseType = MessageTypes.NONE;
                }

                handler.Send(data);
            }
            catch (Exception e) {
                handler.Close();
                OnConnectionStateChanged(ConnectionStatuses.DISCONNECTED);
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }

        public void Disconnect() {
            try {
                if (Socket != null && Socket.Connected) {
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();

                    OnConnectionStateChanged(ConnectionStatuses.DISCONNECTED);
                }
            }
            catch (Exception e) {
                Utils.Log(LogLevels.ERROR, e.ToString());
            }
        }

        private void Handle(byte[] packet, Socket clientSocket) {
            ushort packetLength = BitConverter.ToUInt16(packet, 0);
            ushort packetType = BitConverter.ToUInt16(packet, 2);
            Message message = new Message(packet);

            if (message != null) {
                if (message.Type >= MessageTypes.RESPONSE_IMAGE) {
                    ResponseType = message.Type;
                }

                OnMessageReceived(message);
            }
        }
        #endregion
    }
}
