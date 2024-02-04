using _UTIL_;
using System;
using System.Net;

namespace _RUDP_
{
    public partial class RudpConnection : IDisposable
    {
        public readonly RudpSocket socket;
        public readonly IPEndPoint endPoint;
        public IPEndPoint localEnd, publicEnd;

        public readonly ThreadSafe<bool> disposed = new();

        public double lastSend, lastReceive;

        public override string ToString() => $"{socket} (conn:{endPoint.Port})";

        //----------------------------------------------------------------------------------------------------------

        public RudpConnection(in RudpSocket socket, in IPEndPoint endPoint)
        {
            this.socket = socket;
            this.endPoint = endPoint;

            channels = new()
            {
                { 0, toRemote = new(this, 0, 1, null) },
                { 1, fromRemote = new(this, 1, 0, null) },
            };

            ToggleKeepAlive(true);
        }

        //----------------------------------------------------------------------------------------------------------

        public void OnNetworkPush()
        {
            foreach (RudpChannel channel in channels.Values)
                lock (channel)
                    channel.TrySendPeekPaquet();

            if (eKeepAlive != null && !eKeepAlive.MoveNext())
                eKeepAlive = null;
        }

        public void Send(in byte[] buffer)
        {
            lock (this)
                lastSend = Util.TotalMilliseconds;
            socket.SendTo(buffer, endPoint);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;

            if (channels.Count > 0)
            {
                foreach (RudpChannel channel in channels.Values)
                    channel.Dispose();
                lock (channels)
                    channels.Clear();
            }
        }
    }
}