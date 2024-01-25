using System.Net;
using System.Net.Sockets;

namespace _RUDP_
{
    public partial class UdpSocket
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
                    bool newConnection = ToConnection(remoteEndIP, out UdpConnection recConn);
                    recConn.lastReceive = Util.TotalMilliseconds;

                    Header header = new(reader);
                    if (paquetSize < Header.SIZE)
                        Console.WriteLine($"{paquetSize} bytes from {remoteEnd}");
                    else if (header.mask.HasFlag(UdpHeaderM.Reliable))
                    {
                        ushort msglen = (ushort)(paquetSize - Header.SIZE);
                        if (recConn.channels.TryGetValue(header.channelKey, out var channel))
                            lock (channel.stream)
                                channel.stream.Write(BUFFER, Header.SIZE, msglen);
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