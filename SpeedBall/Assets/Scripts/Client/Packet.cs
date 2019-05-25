using System;
using System.IO;

public class Packet
{
    private MemoryStream stream;
    private BinaryWriter writer;

    private static uint packetCounter;

    private uint id;
    public uint Id { get { return id; } }

    public Packet()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
        id = ++packetCounter;
    }

    public Packet(PacketsCommands command, params object[] elements) : this((byte)command, elements)
    {

    }

    public Packet(byte command, params object[] elements) : this()
    {
        writer.Write(command);
        writer.Write(id);

        foreach (object element in elements)
        {
            if (element is int)
                writer.Write((int)element);
            else if (element is float)
                writer.Write((float)element);
            else if (element is byte)
                writer.Write((byte)element);
            else if (element is char)
                writer.Write((char)element);
            else if (element is uint)
                writer.Write((uint)element);
            else if (element is bool)
                writer.Write((bool)element);
            else
                throw new Exception("unknown type");
        }
    }

    public byte[] GetData()
    {
        return stream.ToArray();
    }
}
