using System.Net;

namespace scripts
{
    public partial class NetSocketUDP
    {
        public partial class Connection : IDisposable
        {
            public double lastSend, lastReceive;
            public readonly NetSocketUDP socket;
            public readonly IPEndPoint remoteEnd;

            //----------------------------------------------------------------------------------------------------------

            public Connection(in NetSocketUDP socket, in IPEndPoint remoteEnd)
            {
                this.socket = socket;
                this.remoteEnd = remoteEnd;
            }

            //----------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                socket.connections.Remove(remoteEnd);
            }
        }

        public readonly Dictionary<IPEndPoint, Connection> connections = new();

        //----------------------------------------------------------------------------------------------------------

        public bool ToConnection(in IPEndPoint remoteEnd, out Connection conn)
        {
            lock (connections)
                if (connections.TryGetValue(remoteEnd, out conn))
                    return false;
                else
                {
                    conn = new Connection(this, remoteEnd);
                    connections.Add(remoteEnd, conn);
                    return true;
                }
        }
    }
}