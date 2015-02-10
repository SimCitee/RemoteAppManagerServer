using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManager.Packets
{
    public enum MessageTypes : ushort
    {
        REQUEST_SEARCH = 1,
        REQUEST_PROCESSES = 2,
        REQUEST_NEXT_PROCESS = 3,
        REQUEST_ICONS = 4,
        REQUEST_NEXT_ICON = 5,
        REQUEST_CLOSE = 6,
        REQUEST_KILL_PROCESS = 7,

        RESPONSE_IMAGE = 8,
        RESPONSE_ERROR = 9,
        RESPONSE_PROCESS = 10,
        RESPONSE_PROCESS_END = 11,
        RESPONSE_KILL_SUCCESS = 12,
        RESPONSE_RECEIVED_SUCCESS = 13,

        NONE = 14
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

        public int GetIntegerValue {
            get {
                int value;

                if (this.Data != null && Int32.TryParse(this.Text, out value)) {
                    return value;
                }

                return 0;
            }
        }
    }
}
