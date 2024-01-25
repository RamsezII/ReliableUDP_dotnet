using System.Net;

namespace _RUDP_
{
    public partial class UdpConnection : IDisposable
    {
        public double lastSend, lastReceive;
        public readonly UdpSocket socket;
        public readonly IPEndPoint remoteEnd;

        public readonly Dictionary<byte, UdpChannel> channels = new();

        //----------------------------------------------------------------------------------------------------------

        public UdpConnection(in UdpSocket socket, in IPEndPoint remoteEnd)
        {
            this.socket = socket;
            this.remoteEnd = remoteEnd;
        }

        //----------------------------------------------------------------------------------------------------------



        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            socket.connections.Remove(remoteEnd);
        }
    }
}