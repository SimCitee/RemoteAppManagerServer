using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Drawing;
using RemoteAppManager.Core;

namespace RemoteAppManager
{
    public delegate void ConnectionStateChangedEventHandler(object sender, EventArgs e);
    public delegate void MessageReceivedEventHandler(Message message);

    public class ConnectionService : IConnectionService
    {
        #region Constants
        public const int PORT = 9999;

        public const String DATA_START_DELIMITER = "<DATA>";
        public const String DATA_END_DELIMITER = "</DATA>";
        public const String RESPONSE_START_DELIMITER = "<RESPONSE>";
        public const String RESPONSE_END_DELIMITER = "</RESPONSE>";
        public const String MESSAGETYPE_START_DELIMITER = "<MSGTYPE>";
        public const String MESSAGETYPE_END_DELIMITER = "</MSGTYPE>";
        #endregion

        public event ConnectionStateChangedEventHandler ConnectionStateChanged;
        public event MessageReceivedEventHandler MessageReceived;

        //private static ManualResetEvent _sendDone = new ManualResetEvent(false);
        //private static ManualResetEvent _receiveDone = new ManualResetEvent(false);
        private StateObject _state;

        #region View properties
        public StateObject State {
            get { return _state; }
        }

        public Boolean IsConnected {
            get {
                return State.IsSocketConnected();
            }
        }
        #endregion

        #region Class
        public ConnectionService() {
            _state = new StateObject();
        }
        #endregion

        #region Events
        protected virtual void OnConnectionStateChanged(EventArgs e) {
            if (ConnectionStateChanged != null) {
                ConnectionStateChanged(this, e);
            }
        }

        protected virtual void OnMessageReceived(Message message) {
            if (MessageReceived != null) {
                MessageReceived(message);
            }
        }
        #endregion

        #region Methods
        public void BeginReceive(Socket handler) {
            handler.BeginReceive(State.Buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), State);
        }

        public void ReadCallback(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            try {
                if (!state.IsSocketConnected()) {
                    handler.Close();
                    OnConnectionStateChanged(EventArgs.Empty);
                    return;
                }

                int byteRead = handler.EndReceive(ar);

                if (byteRead > 0) {
                    state.Builder.Append(Encoding.UTF8.GetString(state.Buffer, 0, byteRead));

                    if (state.Builder.ToString().Contains(RESPONSE_END_DELIMITER)) {
                        Message message = BuildMessage(state.Builder.ToString());
                        if (message != null) {
                            OnMessageReceived(message);
                        }

                        state.Builder.Clear();
                    }
                    //else {
                    //    handler.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, 0,
                    //                    new AsyncCallback(ReadCallback), state);
                    //}
                }
            }
            catch (Exception e) {
                OnConnectionStateChanged(EventArgs.Empty);
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
        }

        public void Send(Socket handler, Bitmap bitmap) {
            byte[] byteData = Utils.Combine(Utils.ImageToByteArray(bitmap), Encoding.UTF8.GetBytes(RESPONSE_END_DELIMITER));

            if (byteData != null) {
                Send(handler, byteData);
            }
        }

        public void Send(Socket handler, Message msg) {
            byte[] byteData = BuildData(msg);
            
            if (byteData != null) {
                Send(handler, byteData);
            }
        }

        public void Send(Socket handler, byte[] byteData) {
            try {
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e) {
                handler.Close();
                OnConnectionStateChanged(EventArgs.Empty);
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
        }

        public void SendCallback(IAsyncResult ar) {
            try {
                Socket handler = (Socket)ar.AsyncState;

                int bytes = handler.EndSend(ar);

                BeginReceive(handler);
            }
            catch (Exception e) {
                OnConnectionStateChanged(EventArgs.Empty);
                Utils.Log(Utils.LogLevels.ERROR, e.ToString());
            }
        }

        private byte[] BuildData(Message msg) {
            String dataText = RESPONSE_START_DELIMITER;

            dataText += MESSAGETYPE_START_DELIMITER + msg.MessageType + MESSAGETYPE_END_DELIMITER;

            switch (msg.MessageType) {
                case MessageTypes.MESSAGE_KILL_PROCESS:
                case MessageTypes.MESSAGE_KILL_SUCCESS:
                case MessageTypes.MESSAGE_PROCESS:
                case MessageTypes.MESSAGE_TEXT:
                    if (msg.Data != null) {
                        dataText += DATA_START_DELIMITER + msg.Data.ToString() + DATA_END_DELIMITER;
                    }
                    break;
            }

            dataText += RESPONSE_END_DELIMITER;

            byte[] data = Encoding.UTF8.GetBytes(dataText);

            return data;
        }

        private Message BuildMessage(String data) {
            String messageTypeText = GetMessageType(data);

            if (!String.IsNullOrEmpty(messageTypeText)) {

                MessageTypes messageType = (MessageTypes)Enum.Parse(typeof(MessageTypes), messageTypeText, false);
                Message msg = new Message(messageType);

                switch (messageType) {
                    case MessageTypes.MESSAGE_KILL_PROCESS:
                    case MessageTypes.MESSAGE_KILL_SUCCESS:
                    case MessageTypes.MESSAGE_PROCESS:
                    case MessageTypes.MESSAGE_TEXT:
                        msg.Data = GetMessageData(data);
                        break;
                }

                return msg;
            }

            return null;
        }

        private String GetMessageResponse(String msg) {
            return GetMessageSegment(msg, RESPONSE_START_DELIMITER, RESPONSE_END_DELIMITER);
        }

        private String GetMessageType(String msg) {
            return GetMessageSegment(msg, MESSAGETYPE_START_DELIMITER, MESSAGETYPE_END_DELIMITER);
        }

        private String GetMessageData(String msg) {
            return GetMessageSegment(msg, DATA_START_DELIMITER, DATA_END_DELIMITER);
        }

        private String GetMessageSegment(String msg, String startDelimiter, String endDelimiter) {
            int indexStart, indexEnd = 0;
            String data = "";

            indexStart = msg.IndexOf(startDelimiter, StringComparison.InvariantCultureIgnoreCase);
            if (indexStart > -1) {
                indexEnd = msg.IndexOf(endDelimiter, indexStart + 1, StringComparison.InvariantCultureIgnoreCase);
                if (indexEnd > -1) {
                    data = msg.Substring(indexStart + startDelimiter.Length, indexEnd - indexStart - startDelimiter.Length);
                }
            }
            return data;
        }

        public void Close() {
            if (State.WorkSocket != null && State.WorkSocket.Connected) {
                State.WorkSocket.Close();
            }
        }
        #endregion
    }
}
