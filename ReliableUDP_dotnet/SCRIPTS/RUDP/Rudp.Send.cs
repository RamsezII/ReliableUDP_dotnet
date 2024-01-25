using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class UdpSocket
    {
        public double lastSend;
        public uint send_count, send_size;

        //----------------------------------------------------------------------------------------------------------

        public void SendTo(in byte[] buffer, in int offset, in int size, in SocketFlags socketFlags, in IPEndPoint targetEnd)
        {
            lock (buffer)
            {
                lastSend = Util.TotalMilliseconds;
                ++send_count;
                send_size += (uint)size;
                SendTo(buffer, offset, size, socketFlags, targetEnd);
            }
        }
    }
}