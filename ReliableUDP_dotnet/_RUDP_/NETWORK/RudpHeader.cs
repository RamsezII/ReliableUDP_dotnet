namespace _RUDP_
{
    enum RudpHeaderB : byte
    {
        reliable,
        ack,
        reset,
        open,
        close,
        _last_,
    }

    [Flags]
    public enum RudpHeaderM : byte
    {
        Reliable = 1 << RudpHeaderB.reliable,
        Ack = 1 << RudpHeaderB.ack,
        Reset = 1 << RudpHeaderB.reset,
        Open = 1 << RudpHeaderB.open,
        Close = 1 << RudpHeaderB.close,
        OpenReliable = Open | Reliable,
        CloseReliable = Close | Reliable,
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