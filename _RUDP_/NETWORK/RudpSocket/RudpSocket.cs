using _UTIL_;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket : Socket, IDisposable
    {
        public const ushort
            BUFFER_SIZE = 1472,
            DATA_SIZE = BUFFER_SIZE - RudpHeader.RELIABLE_SIZE;

        public readonly byte[] PAQUET_BUFFER = new byte[BUFFER_SIZE];
        readonly byte[] ACK_BUFFER = new byte[RudpHeader.RELIABLE_SIZE];

        public readonly MemoryStream stream;
        public readonly BinaryWriter writer;
        public readonly BinaryReader reader;

        public bool HasNext() => stream.Position < reclength_u;
        public ushort Remaining() => (ushort)(reclength_u - stream.Position);

        public readonly ushort localPort;
        readonly EndPoint netpReceive;
        public readonly IPEndPoint netpLoopback, netpLAN;
        public IPEndPoint publicEnd;

        public readonly ThreadSafe<bool> disposed = new();

        public static bool
            logEmptyPaquets = false,
            logAllPaquets = false;

        public readonly Action<BinaryReader> onUnreliableData;
        public readonly Action<RudpChannel, BinaryReader> onNewChannel;

        public override string ToString() => $"(socket {Util.localIP}:{localPort})";

        //----------------------------------------------------------------------------------------------------------

        public RudpSocket(in Action<BinaryReader> onUnreliableData, in Action<RudpChannel, BinaryReader> onNewChannel) : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            stream = new(PAQUET_BUFFER);
            writer = new(stream, Encoding.UTF8, true);
            reader = new(stream, Encoding.UTF8, true);

            this.onUnreliableData = onUnreliableData;
            this.onNewChannel = onNewChannel;

            ExclusiveAddressUse = false;
            SendTo(PAQUET_BUFFER, 0, 0, SocketFlags.None, Util.END_LOOPBACK);
            netpReceive = LocalEndPoint;
            localPort = (ushort)((IPEndPoint)netpReceive).Port;
            netpLoopback = new(IPAddress.Loopback, localPort);
            netpLAN = new(Util.localIP, localPort);
            Debug.Log($"opened UDP: {this}".ToSubLog());
            BeginReceive();
        }

        //----------------------------------------------------------------------------------------------------------

        public new void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;

            Debug.Log($"closed UDP: {this}".ToSubLog());

            eveClient?.Dispose();

            base.Dispose();
            Close();

            if (punchers.Count > 0)
            {
                foreach (RudpPuncher connector in punchers.Values)
                    connector.Dispose();
                lock (punchers)
                    punchers.Clear();
            }

            if (connections.Count > 0)
            {
                foreach (RudpConnection conn in connections.Values)
                    conn.Dispose();
                lock (connections)
                    connections.Clear();
            }
        }
    }
}