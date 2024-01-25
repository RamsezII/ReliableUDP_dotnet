namespace scripts
{
    public partial class NetSocketUDP
    {
        public enum Codes : byte
        {
            Unleriable,
            UnleriableReset,
            Reliable,
        }

        public readonly struct Header
        {
            public const byte SIZE = 4;
            public readonly byte version;
            public readonly Codes code;
            public readonly byte id1, id2;

            //----------------------------------------------------------------------------------------------------------

            public Header(in BinaryReader reader)
            {
                version = reader.ReadByte();
                code = (Codes)reader.ReadByte();
                id1 = reader.ReadByte();
                id2 = reader.ReadByte();
            }

            public Header(in Codes code, in byte id1, in byte id2)
            {
                version = Util_net.VERSION;
                this.code = code;
                this.id1 = id1;
                this.id2 = id2;
            }

            //----------------------------------------------------------------------------------------------------------

            public void Write(in BinaryWriter writer)
            {
                writer.Write(version);
                writer.Write((byte)code);
                writer.Write(id1);
                writer.Write(id2);
            }
        }
    }
}