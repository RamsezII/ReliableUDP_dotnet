using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastSend;
        public uint send_count, send_size;

        //----------------------------------------------------------------------------------------------------------

        public void SendEmptyTo(in IPEndPoint targetEnd) => SendTo(Util.EMPTY_BUFFER, targetEnd);
        public void SendAckTo(in RudpHeader header, in IPEndPoint targetEnd)
        {
            lock (ACK_BUFFER)
            {
                header.WriteToBuffer(ACK_BUFFER);
                SendTo(ACK_BUFFER, targetEnd);
            }
        }

#if UNITY_EDITOR
        [Obsolete("Use indexplage instead")]
        public void SendTo(in byte[] buffer, in int offset, in int size, in IPEndPoint targetEnd) => SendTo(buffer[offset..size], targetEnd);
#endif
        public void SendTo(byte[] buffer, in IPEndPoint targetEnd)
        {
            if (disposed.Value)
                return;

            if (targetEnd.Port == localPort)
                throw new Exception($"{this} {nameof(SendTo)}: Cannot send to self");

            lock (this)
            {
                lastSend = Util.TotalMilliseconds;
                ++send_count;
                send_size += (uint)buffer.Length;
            }

            if (logEmptyPaquets || logAllPaquets && buffer.Length > 0)
                if (buffer.Length >= RudpHeader.RELIABLE_SIZE)
                    Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} (header:{RudpHeader.FromBuffer(buffer)}, size:{buffer.Length})".ToSubLog());
                else
                    Debug.Log($"{this} {nameof(SendTo)}: {targetEnd} (size:{buffer.Length})".ToSubLog());

            SendTo(buffer, 0, buffer.Length, SocketFlags.None, targetEnd);
        }
    }
}