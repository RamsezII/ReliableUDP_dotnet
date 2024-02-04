using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpConnection
    {
        public readonly Dictionary<byte, RudpChannel> channels;

        public RudpChannel toRemote, fromRemote;
        byte channelIdIncrement;

        //----------------------------------------------------------------------------------------------------------

        public void TryAddChannel(Action<RudpChannel> onChannel, Action onFailure)
        {
            RudpChannel channel = null;

            lock (channels)
                if (channels.TryReserveKey(ref channelIdIncrement, true, 2))
                    channel = channels[channelIdIncrement] = new(this, channelIdIncrement, 0, null);

            if (channel == null)
            {
                Debug.LogWarning($"{this} Can not add new channel");
                onFailure?.Invoke();
                return;
            }

            channel.onSuccess = success =>
            {
                if (success)
                    onChannel(channel);
                else
                {
                    lock (channels)
                        channels.Remove(channel.local_id);
                    onFailure?.Invoke();
                }
            };

            channel.WriteAndSend(writer => writer.Write(channelIdIncrement), true, RudpHeaderM.AddChannel);
        }

        bool TryReceiveNewChannel(in RudpHeader header, in byte remote_id)
        {
            bool success;
            byte local_id;

            lock (channels)
            {
                success = channels.TryReserveKey(ref channelIdIncrement, true, 2);
                local_id = channelIdIncrement;
            }

            if (success)
            {
                Debug.Log($"{socket} New channel:{local_id} with remote id:{remote_id}");

                RudpChannel channel = new(this, local_id, remote_id, null);
                lock (channels)
                    channels[local_id] = channel;
                channel.WriteAndSend(writer => writer.Write(local_id), true, RudpHeaderM.ConfirmChannel);

                socket.onNewChannel(channel, socket.reader);

                return true;
            }
            else
            {
                Debug.LogError($"{socket} Received paquet for new channel({remote_id}) but no more channel available");
                socket.stream.Position = socket.reclength_u;

                RudpHeader header2 = new(RudpHeaderM.ConfirmChannel, remote_id, header.id);
                byte[] buffer = new byte[RudpHeader.RELIABLE_SIZE];
                header2.WriteToBuffer(buffer);

                lock (fromRemote.pendingPaquets)
                    fromRemote.pendingPaquets.Enqueue(new(header2, buffer));

                return false;
            }
        }

        bool OnChannelConfirmation(in RudpHeader header, in byte remote_id)
        {
            RudpChannel channel;
            lock (channels)
                channels.TryGetValue(header.id, out channel);

            if (channel == null)
            {
                Debug.LogError($"{socket} Received paquet for unknown channel:{remote_id}");
                return false;
            }

            lock (channel)
            {
                channel.remote_id = remote_id;
                if (remote_id >= 2)
                {
                    Debug.Log($"{socket} Channel:{channel} added");
                    channel.onSuccess(true);
                    return true;
                }
                else
                {
                    Debug.LogError($"{socket} Channel:{channel} failed to be added (code: {remote_id})");
                    channel.onSuccess(false);
                    channel.Dispose();
                    channels.Remove(header.id);
                }
                channel.onSuccess = null;
            }
            return false;
        }
    }
}