using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public enum PacketsCommands
    {
        Join = 0,
        Welcome = 1,
        Spawn = 2,
        Update = 3,
        Input = 4,
        GameInfo = 5,
        Pong = 253,
        Ping = 254,
        Ack = 255
    }

    public class Packet
    {
        private MemoryStream stream;
        private BinaryWriter writer;

        private static uint packetCounter;

        public float SendAfter;
        public bool OneShot;

        public bool NeedAck;

        private uint id;
        public uint Id
        {
            get
            {
                return id;
            }
        }

        private float expires;
        public bool IsExpired(float now)
        {
            return expires < now;
        }

        public void SetExpire(float death)
        {
            expires = death;
        }

        private uint attempts;
        public uint Attempts
        {
            get
            {
                return attempts;
            }
        }

        public Packet(bool needAck=false)
        {
            NeedAck = needAck;
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
            id = ++packetCounter;
            attempts = 0;
            OneShot = false;
            SendAfter = 0;
        }

        public Packet(PacketsCommands command, bool needAck = false, params object[] elements) : this((byte)command,needAck,elements)
        {

        }

        public Packet(byte command,bool needAck=false, params object[] elements) : this(needAck)
        {
            // first element is always the command (1 byte)
            writer.Write(command);

            //second element is always the packet id (4 bytes)
            writer.Write(this.id);

            foreach (object element in elements)
            {
                if (element is int)
                {
                    writer.Write((int)element);
                }
                else if (element is float)
                {
                    writer.Write((float)element);
                }
                else if (element is byte)
                {
                    writer.Write((byte)element);
                }
                else if (element is char)
                {
                    writer.Write((char)element);
                }
                else if (element is uint)
                {
                    writer.Write((uint)element);
                }
                else if (element is bool)
                {
                    writer.Write((bool)element);
                }
                else if (element is short)
                {
                    writer.Write((short)element);
                }
                else
                {
                    throw new Exception("unknown type");
                }
            }
        }

        public byte[] GetData()
        {
            return stream.ToArray();
        }

        public void IncreaseAttempts()
        {
            attempts++;
        }
    }
}
