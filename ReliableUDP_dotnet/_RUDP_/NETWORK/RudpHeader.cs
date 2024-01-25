namespace _RUDP_
{
    enum RudpHeaderB : byte
    {
        unreliable,
        reliable,
        reset,
        ack,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Unreliable = 1 << RudpHeaderB.unreliable,
        Reliable = 1 << RudpHeaderB.reliable,
        Reset = 1 << RudpHeaderB.reset,
        ReliableReset = Reliable | Reset,
        Ack = 1 << RudpHeaderB.ack,
    }

    public readonly struct RudpHeader
    {
        public const byte SIZE = 4;
        public readonly byte version;
        public readonly RudpHeaderM mask;
        public readonly byte paquetID, channelKey;

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in BinaryReader reader)
        {
            version = reader.ReadByte();
            mask = (RudpHeaderM)reader.ReadByte();
            paquetID = reader.ReadByte();
            channelKey = reader.ReadByte();
        }

        public RudpHeader(in RudpHeaderM mask, in byte paquetID, in byte channelKey)
        {
            version = Util.VERSION;
            this.mask = mask;
            this.paquetID = paquetID;
            this.channelKey = channelKey;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Write(in BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write((byte)mask);
            writer.Write(paquetID);
            writer.Write(channelKey);
        }
    }
}