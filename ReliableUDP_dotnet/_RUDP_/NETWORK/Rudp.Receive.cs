using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastReceive;
        public uint receive_count, receive_size;
        bool skipFirstSocketException = true;

        //----------------------------------------------------------------------------------------------------------

        void ReceiveFrom(IAsyncResult aResult)
        {
            try
            {
                lock (BUFFER)
                {
                    lastReceive = Util.TotalMilliseconds;
                    ++receive_count;
                    EndPoint remoteEnd = localIP!;
                    stream.Position = 0;
                    ushort paquetSize = (ushort)EndReceiveFrom(aResult, ref remoteEnd);
                    receive_size += paquetSize;

                    IPEndPoint remoteEndIP = (IPEndPoint)remoteEnd;
                    bool newConnection = ToConnection(remoteEndIP, out RudpConnection recConn);
                    recConn.lastReceive = Util.TotalMilliseconds;

                    RudpHeader header = new(reader);
                    if (paquetSize < RudpHeader.SIZE)
                        Console.WriteLine($"{paquetSize} bytes from {remoteEnd}");
                    else if (header.mask.HasFlag(RudpHeaderM.Reliable))
                    {
                        ushort msglen = (ushort)(paquetSize - RudpHeader.SIZE);
                        if (recConn.channels.TryGetValue(header.channelKey, out var channel))
                            lock (channel.stream)
                                channel.stream.Write(BUFFER, RudpHeader.SIZE, msglen);
                    }
                }
            }
            catch (SocketException e)
            {
                if (skipFirstSocketException)
                    skipFirstSocketException = false;
                else
                    Console.WriteLine(e.Message.Trim('\n', '\t', '\r'));
            }
            catch (Exception e) { Console.WriteLine(e.Message.Trim('\n', '\t', '\r')); }
            BeginReceive();
        }

        void BeginReceive()
        {
            EndPoint receiveEnd = localIP!;
            try { BeginReceiveFrom(BUFFER, 0, BUFFER_SIZE, SocketFlags.None, ref receiveEnd, ReceiveFrom, null); }
            catch (Exception e) { Console.WriteLine(e.Message.Trim('\n', '\t', '\r')); }
        }
    }
}