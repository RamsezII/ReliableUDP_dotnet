using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class RudpSocket : Socket
    {
        const ushort BUFFER_SIZE = 1472;
        static readonly byte[] BUFFER = new byte[BUFFER_SIZE];
        static readonly MemoryStream stream = new(BUFFER);
        static readonly BinaryReader reader = new(stream);
        public readonly IPEndPoint localIP;
        public readonly Dictionary<IPEndPoint, RudpConnection> connections = new();

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in bool skipFirstSocketException = false) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            this.skipFirstSocketException = skipFirstSocketException;
            ExclusiveAddressUse = false;
            SendTo(BUFFER, 0, 0, SocketFlags.None, Util.END_LOOPBACK);
            localIP = (IPEndPoint)LocalEndPoint;
            BeginReceive();
        }

        //----------------------------------------------------------------------------------------------------------

        public bool ToConnection(in IPEndPoint remoteEnd, out RudpConnection conn)
        {
            lock (connections)
                if (connections.TryGetValue(remoteEnd, out conn))
                    return false;
                else
                {
                    conn = new RudpConnection(this, remoteEnd);
                    connections.Add(remoteEnd, conn);
                    return true;
                }
        }
    }
}