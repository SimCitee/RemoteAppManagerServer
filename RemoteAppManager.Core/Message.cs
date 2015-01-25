using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManager
{
    public enum MessageTypes
    {
        MESSAGE_REQUEST_SEARCH = 1,
        MESSAGE_REQUEST_PROCESSES = 2,
        MESSAGE_TEXT = 3,
        MESSAGE_IMAGE = 4,
        MESSAGE_SUCCESS = 5,
        MESSAGE_ERROR = 6,
        MESSAGE_CLOSE = 7,
        MESSAGE_PROCESS = 8,
        MESSAGE_NONE = 9
    }

    public class Message
    {

        private Object _data;
        private MessageTypes _messageType;

        public Object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public MessageTypes MessageType {
            get { return _messageType; }
            set { _messageType = value; }
        }

        public Message() : this(MessageTypes.MESSAGE_NONE) { }

        public Message(MessageTypes messageType) : this(messageType, null) { }

        public Message(MessageTypes messageType, Object data) {
            _messageType = messageType;
            _data = data;
        }
    }
}
