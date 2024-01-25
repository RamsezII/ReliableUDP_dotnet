namespace _RUDP_
{
    public class UdpChannel : IDisposable
    {
        public readonly MemoryStream stream;
        public readonly BinaryReader reader;
        public readonly BinaryWriter writer;
        public readonly byte id;
        public readonly UdpConnection conn;

        //----------------------------------------------------------------------------------------------------------

        public UdpChannel(in byte id, in UdpConnection conn)
        {
            this.id = id;
            this.conn = conn;
            stream = new();
            reader = new(stream);
            writer = new(stream);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            conn.channels.Remove(id);
        }
    }
}