using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManager.Packets
{
    public abstract class PacketStructure
    {
        public static int BUFFER_SIZE = 3072;

        private byte[] _buffer;

        public PacketStructure(ushort length, MessageTypes type): this(length, Convert.ToUInt16(type)) {}

        public PacketStructure(ushort length, ushort type) {
            _buffer = new byte[length];
            WriteUShort(length, 0);
            WriteUShort(type, 2);
        }

        public PacketStructure(byte[] packet) {
            _buffer = packet;
        }

        public ushort ReadUShort(int offset) {
            return BitConverter.ToUInt16(_buffer, offset);
        }

        public void WriteUShort(ushort value, int offset) {
            byte[] tempBuffer = new byte[2];
            tempBuffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(tempBuffer, 0, _buffer, offset, 2);
        }

        public void WriteString(String value, int offset) {
            byte[] tempBuffer = new byte[value.Length];
            tempBuffer = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(tempBuffer, 0, _buffer, offset, value.Length);
        }

        public String ReadString(int offset, int count) {
            return Encoding.UTF8.GetString(_buffer, offset, count);
        }

        public byte[] Data {
            get {
                return _buffer;
            }
        }
    }
}
