using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManager.Packets
{
    public enum MessageTypes : ushort
    {
        MESSAGE_REQUEST_SEARCH = 1,
        MESSAGE_REQUEST_PROCESSES = 2,
        MESSAGE_TEXT = 3,
        MESSAGE_IMAGE = 4,
        MESSAGE_ERROR = 5,
        MESSAGE_CLOSE = 6,
        MESSAGE_PROCESS = 7,
        MESSAGE_NONE = 8,
        MESSAGE_KILL_PROCESS = 9,
        MESSAGE_KILL_SUCCESS = 10,
        MESSAGE_REQUEST_PROCESSES_END = 11,
        MESSAGE_REQUEST_ICONS = 12
    }

    public class Message : PacketStructure
    {
        private MessageTypes _messageType;
        private String _text;

        public Message(MessageTypes type)
            : this(type, "") {
        }

        public Message(MessageTypes type, String message)
            : base((ushort)(4 + message.Length), (ushort)type) {
            Type = type;
            Text = message;
        }

        public Message(byte[] packet)
            : base(packet) { }

        public MessageTypes Type {
            get { return (MessageTypes)ReadUShort(2); }
            set {
                _messageType = value;
                WriteUShort((ushort)_messageType, 2);
            }
        }

        public String Text {
            get { return ReadString(4, Data.Length - 4); }
            set {
                _text = value;
                WriteString(value, 4);
            }
        }
    }
}
