using System.Net;
using System.Net.Sockets;

namespace scripts
{
    public partial class NetSocketUDP : Socket
    {
        public const ushort BUFFER_SIZE = 1472;
        static readonly byte[] BUFFER = new byte[BUFFER_SIZE];
        static readonly DoubleStream stream = new(BUFFER);
        public readonly IPEndPoint? localIP;

        //----------------------------------------------------------------------------------------------------------

        public NetSocketUDP(in bool skipFirstSocketException = false) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            this.skipFirstSocketException = skipFirstSocketException;
            ExclusiveAddressUse = false;
            SendTo(BUFFER, 0, 0, SocketFlags.None, Util_net.END_LOOPBACK);
            localIP = (IPEndPoint?)LocalEndPoint;
            BeginReceive();
        }
    }
}