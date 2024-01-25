using System.Net;

namespace _RUDP_
{
    public partial class RudpConnection : IDisposable
    {
        public double lastSend, lastReceive;
        public readonly RudpSocket socket;
        public readonly IPEndPoint remoteEnd;
        public readonly Dictionary<byte, RudpChannel> channels = new();

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint remoteEnd)
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
}