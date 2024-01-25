namespace scripts
{
    public partial class NetSocketUDP
    {
        public partial class Connection
        {
            public class Channel : IDisposable
            {
                public readonly MemoryStream buffer = new();
                public readonly byte id;
                public readonly Connection conn;

                //----------------------------------------------------------------------------------------------------------

                public Channel(in byte id, in Connection conn)
                {
                    this.id = id;
                    this.conn = conn;
                }

                //----------------------------------------------------------------------------------------------------------

                public void Dispose()
                {
                    conn.channels.Remove(id);
                }
            }

            public readonly Dictionary<byte, Channel> channels = new();
        }
    }
}