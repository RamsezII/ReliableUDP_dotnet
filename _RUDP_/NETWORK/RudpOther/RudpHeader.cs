using System;
using System.IO;

namespace _RUDP_
{
    public enum RudpHeaderI : byte
    {
        Version,
        Mask,
        Channel,
        ID,
        Attempt,
        _last_,
    }

    enum RudpHeaderB : byte
    {
        eve,
        reliable,
        ack,
        start,
        accept,
        kill,
        transfer,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Eve = 1 << RudpHeaderB.eve,
        Reliable = 1 << RudpHeaderB.reliable,
        Ack = 1 << RudpHeaderB.ack,
        Start = 1 << RudpHeaderB.start,
        Accept = 1 << RudpHeaderB.accept,
        Kill = 1 << RudpHeaderB.kill,
        Transfer = 1 << RudpHeaderB.transfer,

        ReliableAck = Reliable | Ack,
        AddChannel = Reliable | Start,
        ConfirmChannel = Reliable | Accept,
        CloseChannel = Reliable | Kill,
        ReliableTransfer = Reliable | Transfer,
    }

    public readonly struct RudpHeader
    {
        public const byte RELIABLE_SIZE = (byte)RudpHeaderI._last_;

        public readonly byte version;
        public readonly RudpHeaderM mask;
        public readonly byte channel, id, attempt;

        public override string ToString() => $"{{ve:{version} ma:{{{mask}}} ch:{channel} id:{id} at:{attempt}}}";

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in RudpHeaderM mask, in byte channel, in byte id) : this(Util.VERSION, mask, channel, id, 0)
        {
        }

        RudpHeader(in byte version, in RudpHeaderM mask, in byte channel, in byte id, in byte attempt)
        {
            this.version = version;
            this.mask = mask;
            this.channel = channel;
            this.id = id;
            this.attempt = attempt;
        }

        //----------------------------------------------------------------------------------------------------------

        public static RudpHeader FromBuffer(in byte[] buffer) => new(buffer[0], (RudpHeaderM)buffer[1], buffer[2], buffer[3], buffer[4]);

        public static RudpHeader FromReader(in BinaryReader reader) => new(reader.ReadByte(), (RudpHeaderM)reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public void Write(in BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write((byte)mask);
            writer.Write(channel);
            writer.Write(id);
            writer.Write(attempt);
        }

        public void WriteToBuffer(in byte[] buffer)
        {
            buffer[0] = version;
            buffer[1] = (byte)mask;
            buffer[2] = channel;
            buffer[3] = id;
            buffer[4] = attempt;
        }
    }
}