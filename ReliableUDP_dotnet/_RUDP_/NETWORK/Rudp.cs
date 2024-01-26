using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class RudpSocket : Socket
    {
        public const ushort BUFFER_SIZE = 1472;
        public static readonly byte[] BUFFER = new byte[BUFFER_SIZE];
        public static readonly MemoryStream stream = new(BUFFER);
        public static readonly BinaryWriter writer = new(stream);
        public static readonly BinaryReader reader = new(stream);
        public readonly IPEndPoint localEndIP;
        public readonly Dictionary<IPEndPoint, RudpConnection> connections = [];

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in bool skipFirstSocketException = false) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            this.skipFirstSocketException = skipFirstSocketException;
            ExclusiveAddressUse = false;
            SendTo(BUFFER, 0, 0, SocketFlags.None, Util.END_LOOPBACK);
            localEndIP = (IPEndPoint)LocalEndPoint;
            BeginReceive();
        }

        public override string ToString() => $"(socket:{localEndIP.Port})";

        //----------------------------------------------------------------------------------------------------------

        public bool ToConnection(in IPEndPoint remoteEnd, out RudpConnection conn)
        {
            lock (connections)
                if (connections.TryGetValue(remoteEnd, out conn))
                    return false;
                else
                {
                    Console.WriteLine($"New connection to {remoteEnd}");
                    connections.Add(remoteEnd, conn = new RudpConnection(this, remoteEnd, null));
                    return true;
                }
        }
    }
}