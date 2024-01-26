using System.Net;

namespace _RUDP_
{
    public partial class RudpConnection : IDisposable
    {
        byte last_sendID, last_recID;
        public double lastSend, lastReceive;
        public readonly RudpSocket socket;
        public readonly IPEndPoint remoteEnd;
        public readonly Dictionary<byte, RudpChannel> channels;
        public readonly RudpChannel stdin, stdout;
        byte lastChannelID;
        readonly BinaryReader incomingUnreliableStream;
        public readonly Action<BinaryReader, ushort> onUnreliablePaquetDirectRead;

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint remoteEnd, in Action<BinaryReader, ushort> onUnreliablePaquetDirectRead)
        {
            this.socket = socket;
            this.remoteEnd = remoteEnd;

            channels = new(){
                { 0, stdin = new(0, this) },
                { 1, stdout = new(1, this) },
            };
            lastChannelID = 1;

            this.onUnreliablePaquetDirectRead = onUnreliablePaquetDirectRead;
            if (onUnreliablePaquetDirectRead == null)
                incomingUnreliableStream = new(new MemoryStream());
        }

        //----------------------------------------------------------------------------------------------------------

        public bool TryAddNewChannel(out RudpChannel? channel)
        {
            if (channels.Count < byte.MaxValue - 1)
            {
                if (lastChannelID == 0)
                    lastChannelID = 1;

                lock (channels)
                {
                    while (channels.ContainsKey(++lastChannelID)) ;
                    channel = channels[lastChannelID] = new(lastChannelID, this);
                }
                return true;
            }

            Console.WriteLine("Too many channels");
            channel = default;
            return false;
        }

        public bool TryAcceptPaquet(in RudpHeader header, in ushort msglen)
        {
            if (header.paquetID != 1 + last_recID)
                return false;
            else
            {
                if (header.unreliable)
                    if (onUnreliablePaquetDirectRead != null)
                        onUnreliablePaquetDirectRead(RudpSocket.reader, msglen);
                    else
                        lock (incomingUnreliableStream)
                            incomingUnreliableStream.BaseStream.Write(RudpSocket.BUFFER, RudpHeader.SIZE, msglen);
                else
                {
                    if (header.paquetID != 1 + last_recID)
                        return false;

                    RudpChannel? channel;
                    lock (channels)
                        if (!channels.TryGetValue(header.channelKey, out channel))
                            if (header.mask.HasFlag(RudpHeaderM.Open))
                                channels.Add(header.channelKey, channel = new(header.channelKey, this));
                            else
                                return false;

                    lock (channel.stream)
                        channel.stream.Write(RudpSocket.BUFFER, RudpHeader.SIZE, msglen);
                }

                last_recID = header.paquetID;
                return true;
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            socket.connections.Remove(remoteEnd);
        }
    }
}