using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastSend;
        public uint send_count, send_size;

        //----------------------------------------------------------------------------------------------------------

        public void SendTo(in IPEndPoint targetEnd)
        {
            SendTo(BUFFER, 0, (int)stream.Position, targetEnd);
            stream.Position = 0;
        }

        public void SendTo(in byte[] buffer, in int offset, in int size, in IPEndPoint targetEnd)
        {
            lock (buffer)
            {
                lastSend = Util.TotalMilliseconds;
                ++send_count;
                send_size += (uint)size;
                SendTo(buffer, offset, size, SocketFlags.None, targetEnd);
            }
        }
    }
}