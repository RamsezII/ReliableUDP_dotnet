namespace _RUDP_
{
    enum RudpHeaderB : byte
    {
        eve,
        ack,
        reliable,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Eve = 1 << RudpHeaderB.eve,
        Ack = 1 << RudpHeaderB.ack,
        Reliable = 1 << RudpHeaderB.reliable,
    }

    public readonly struct RudpHeader
    {
        public readonly bool unreliable;
        public const byte SIZE = 4;
        public readonly byte version;
        public readonly RudpHeaderM mask;
        public readonly byte paquetID, channelKey;

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader(in RudpHeaderM mask, in byte paquetID, in byte channelKey) : this(Util.VERSION, mask, paquetID, channelKey)
        {
        }

        public RudpHeader(in BinaryReader reader) : this(reader.ReadByte(), (RudpHeaderM)reader.ReadByte(), reader.ReadByte(), reader.ReadByte())
        {
        }

        RudpHeader(in byte version, in RudpHeaderM mask, in byte paquetID, in byte channelKey)
        {
            this.version = version;
            this.mask = mask;
            this.paquetID = paquetID;
            this.channelKey = channelKey;
            unreliable = (mask & RudpHeaderM.Reliable) == 0;
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