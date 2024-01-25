namespace _RUDP_
{
    enum UdpHeaderB : byte
    {
        unreliable,
        reliable,
        reset,
        ack,
    }

    [Flags]
    public enum UdpHeaderM : byte
    {
        Unreliable = 1 << UdpHeaderB.unreliable,
        Reliable = 1 << UdpHeaderB.reliable,
        Reset = 1 << UdpHeaderB.reset,
        ReliableReset = Reliable | Reset,
        Ack = 1 << UdpHeaderB.ack,
    }

    public readonly struct Header
    {
        public const byte SIZE = 4;
        public readonly byte version;
        public readonly UdpHeaderM mask;
        public readonly byte paquetID, channelKey;

        //----------------------------------------------------------------------------------------------------------

        public Header(in BinaryReader reader)
        {
            version = reader.ReadByte();
            mask = (UdpHeaderM)reader.ReadByte();
            paquetID = reader.ReadByte();
            channelKey = reader.ReadByte();
        }

        public Header(in UdpHeaderM mask, in byte paquetID, in byte channelKey)
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