namespace _RUDP_
{
    /// <summary>
    /// write, then read, then write, then read, etc... like a TCP stream
    /// </summary>
    public partial class RudpChannel : IDisposable
    {
        public readonly MemoryStream stream;
        public readonly BinaryReader reader;
        public readonly BinaryWriter writer;
        public readonly byte id;
        public readonly RudpConnection conn;

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in byte id, in RudpConnection conn)
        {
            this.id = id;
            this.conn = conn;
            stream = new();
            reader = new(stream);
            writer = new(stream);
        }

        //----------------------------------------------------------------------------------------------------------

        public RudpHeader GetHeader(in RudpHeaderM mask)
        {

        }

        public void Send()
        {

        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            conn.channels.Remove(id);
        }
    }
}