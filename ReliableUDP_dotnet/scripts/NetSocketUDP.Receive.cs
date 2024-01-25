using System.Net;
using System.Net.Sockets;

namespace scripts
{
    public partial class NetSocketUDP
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
                    stream.stream.Position = stream.i_read = 0;
                    ushort paquetSize = stream.i_write = (ushort)EndReceiveFrom(aResult, ref remoteEnd);
                    receive_size += paquetSize;

                    using var rborrow = stream.DisposableRead();
                    Header header = new(rborrow.reader);

                    IPEndPoint remoteEndIP = (IPEndPoint)remoteEnd;
                    bool newConnection = ToConnection(remoteEndIP, out Connection recConn);
                    recConn.lastReceive = Util.TotalMilliseconds;

                    if (paquetSize > 2)
                        if (paquetSize >= Header.SIZE)
                            OnReceive();
                        else
                            Console.WriteLine($"{paquetSize} bytes from {remoteEnd}");

                    rborrow.stream.Reset();
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