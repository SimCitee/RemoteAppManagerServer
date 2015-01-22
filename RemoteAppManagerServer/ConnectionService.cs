using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteAppManagerServer
{
    public delegate void StateChangedEventHandler(object sender, EventArgs e);
    public delegate void MessageReceivedEventHandler(object sender, EventArgs e);

    class ConnectionService : IConnectionService
    {
        private class StateObject
        {
            public const int BUFFER_SIZE = 1024;

            private Socket _socket = null;
            private byte[] _buffer = new byte[BUFFER_SIZE];
            private StringBuilder _stringBuilder = new StringBuilder();

            public Socket WorkSocket
            {
                get { return _socket; }
                set { _socket = value; }
            }

            public byte[] Buffer
            {
                get { return _buffer; }
            }

            public StringBuilder Builder
            {
                get { return _stringBuilder; }
            }

            public bool IsSocketConnected()
            {
                return (_socket != null && !((_socket.Poll(1000, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected));
            }
        }

        public const String DATA_START_DELIMITER = "<DATA>";
        public const String DATA_END_DELIMITER = "</DATA>";
        public const String RESPONSE_START_DELIMITER = "<RESPONSE>";
        public const String RESPONSE_END_DELIMITER = "</RESPONSE>";
        public const String MESSAGETYPE_START_DELIMITER = "<MSGTYPE>";
        public const String MESSAGETYPE_END_DELIMITER = "</MSGTYPE>";

        public enum ConnectionStates
        {
            CONNECTING = 1,
            CONNECTED = 2,
            DISCONNECTED = 3
        }

        public event StateChangedEventHandler StateChanged;
        public event MessageReceivedEventHandler MessageReceived;

        private StateObject _state;
        private ConnectionStates _connectionState;

        #region Properties
        public ConnectionStates ConnectionState
        {
            get { return _connectionState; }
            set
            {
                _connectionState = value;
                OnStateChanged(EventArgs.Empty);
            }
        }
        #endregion

        #region Class
        public ConnectionService()
        {
            _state = new StateObject();
            _connectionState = ConnectionStates.DISCONNECTED;
        }
        #endregion

        #region Events
        protected virtual void OnStateChanged(EventArgs e)
        {
            if (StateChanged != null)
            {
                StateChanged(this, e);
            }
        }
        #endregion

        #region Methods

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            if (state.IsSocketConnected())
            {
                handler.Close();
                this.ConnectionState = ConnectionStates.DISCONNECTED;
                return;
            }

            int byteRead = handler.EndReceive(ar);

            if (byteRead > 0)
            {
                state.Builder.Append(Encoding.UTF8.GetString(state.Buffer, 0, byteRead));

                if (state.Builder.ToString().Contains(RESPONSE_END_DELIMITER))
                {

                }
            }
        }

        public void Send(Socket handler, Message msg)
        {
            byte[] byteData = BuildData(msg);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public void SendCallback(IAsyncResult ar)
        {
        }

        private byte[] BuildData(Message msg)
        {
            String dataText = RESPONSE_START_DELIMITER;

            dataText += MESSAGETYPE_START_DELIMITER + msg.MessageType + MESSAGETYPE_END_DELIMITER;

            switch (msg.MessageType)
            {
                case MessageTypes.MESSAGE_TEXT:
                    if (msg.Data != null)
                    {
                        dataText += DATA_START_DELIMITER + msg.Data.ToString() + DATA_END_DELIMITER;
                    }
                    break;
            }

            dataText += DATA_END_DELIMITER;

            byte[] data = Encoding.UTF8.GetBytes(dataText);

            return data;
        }

        private Message BuildMessage(String data)
        {
            String messageTypeText = GetMessageType(data);

            if (!String.IsNullOrEmpty(messageTypeText))
            {

                MessageTypes messageType = (MessageTypes)Int32.Parse(messageTypeText);
                Message msg = new Message(messageType);

                switch (messageType)
                {
                    case MessageTypes.MESSAGE_TEXT:
                        msg.Data = GetMessageData(data);
                        break;
                }

                return msg;
            }

            return null;
        }

        private String GetMessageResponse(String msg)
        {
            return GetMessageSegment(msg, RESPONSE_START_DELIMITER, RESPONSE_END_DELIMITER);
        }

        private String GetMessageType(String msg)
        {
            return GetMessageSegment(msg, MESSAGETYPE_START_DELIMITER, MESSAGETYPE_END_DELIMITER);
        }

        private String GetMessageData(String msg)
        {
            return GetMessageSegment(msg, DATA_START_DELIMITER, DATA_END_DELIMITER);
        }

        private String GetMessageSegment(String msg, String startDelimiter, String endDelimiter)
        {
            int indexStart, indexEnd = 0;
            String data = "";

            indexStart = msg.IndexOf(startDelimiter, StringComparison.InvariantCultureIgnoreCase);
            if (indexStart > -1)
            {
                indexEnd = data.IndexOf(endDelimiter, indexStart + 1, StringComparison.InvariantCultureIgnoreCase);
                if (indexEnd > -1)
                {
                    data = msg.Substring(indexStart + startDelimiter.Length, indexEnd - indexStart - startDelimiter.Length);
                }
            }
            return data;
        }
        #endregion
    }
}
