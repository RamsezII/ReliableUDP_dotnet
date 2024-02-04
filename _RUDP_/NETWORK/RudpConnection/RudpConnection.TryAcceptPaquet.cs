using UnityEngine;

namespace _RUDP_
{
    public partial class RudpConnection
    {
        public bool TryAcceptPaquet(in RudpHeader header)
        {
            RudpChannel channel;
            lock (channels)
                channels.TryGetValue(header.channel, out channel);

            if (channel == null)
            {
                Debug.LogError($"{socket} Received paquet for unknown channel:{header}");
                return false;
            }

            if (RudpSocket.logAllPaquets)
                Debug.Log($"{channel} Received paquet (header:{header}, size:{socket.reclength_u})".ToSubLog());

            if (header.mask.HasFlag(RudpHeaderM.ReliableAck))
            {
                if (!channel.TryAcceptAck(header))
                    return false;
                channel.TrySendPeekPaquet();
            }
            else
            {
                if (header.id == channel.last_recID + 1)
                    channel.last_recID = header.id < byte.MaxValue ? header.id : (byte)0;
                else
                    return false;

                socket.SendAckTo(new(RudpHeaderM.ReliableAck, channel.remote_id, header.id), endPoint);

                if (header.mask.HasFlag(RudpHeaderM.AddChannel) || header.mask.HasFlag(RudpHeaderM.ConfirmChannel))
                {
                    Debug.Log($"{channel} Received channel management paquet:{header.channel}".ToSubLog());
                    byte channelID = socket.reader.ReadByte();

                    if (header.mask.HasFlag(RudpHeaderM.AddChannel))
                        TryReceiveNewChannel(header, channelID);
                    else if (header.mask.HasFlag(RudpHeaderM.ConfirmChannel))
                        OnChannelConfirmation(header, channelID);
                    else
                        Debug.LogError($"{this} Kept wrongly buffered instruction:{header}");
                }

                if (socket.HasNext())
                    channel.onDirectRead(channel, socket.reader);

                if (socket.HasNext())
                {
                    Debug.Log($"{channel} Buffering incoming data...".ToSubLog());
                    lock (channel.stream)
                    {
                        ushort initialPos = (ushort)channel.stream.Position;
                        channel.stream.Position = channel.stream.Length;
                        channel.writer.Write(socket.PAQUET_BUFFER, (int)socket.stream.Position, socket.reclength_u - (ushort)socket.stream.Position);
                        channel.stream.Position = initialPos;
                        Debug.Log($"{channel} Buffered all incoming data".ToSubLog());
                    }
                    socket.stream.Position = socket.reclength_u;
                }
            }
            return true;
        }
    }
}