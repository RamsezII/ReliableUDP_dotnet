using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public double lastReceive;
        public uint receive_count, receive_size;
        bool skipNextSocketException = true;

        public IPEndPoint recEnd_u;
        public ushort reclength_u;

        //----------------------------------------------------------------------------------------------------------

        void ReceiveFrom(IAsyncResult aResult)
        {
            if (disposed.Value)
                return;
            try
            {
                lock (PAQUET_BUFFER)
                {
                    lastReceive = Util.TotalMilliseconds;
                    ++receive_count;
                    EndPoint remoteEnd = netpReceive;
                    stream.Position = 0;
                    reclength_u = (ushort)EndReceiveFrom(aResult, ref remoteEnd);
                    receive_size += reclength_u;

                    recEnd_u = (IPEndPoint)remoteEnd;
                    bool newConn = false;
                    RudpConnection recConn = null;

                    if (reclength_u == 0)
                        lock (punchers)
                            if (punchers.Count > 0)
                                if (punchers.TryGetValue(recEnd_u, out RudpPuncher puncher))
                                    lock (puncher)
                                    {
                                        Debug.Log($"{puncher.log} (punch success)".ToSubLog());
                                        recConn = ToConnection(recEnd_u, out newConn);
                                        puncher.onSuccess?.Invoke(recConn);
                                        puncher.routine = null;
                                        puncher.Dispose();
                                    }

                    recConn ??= ToConnection(recEnd_u, out newConn);

                    if (newConn && reclength_u == 0)
                        Debug.Log($"(new connection) {recConn}".ToSubLog());

                    recConn.lastReceive = Util.TotalMilliseconds;

                    if (reclength_u >= RudpHeader.RELIABLE_SIZE)
                    {
                        RudpHeader header = RudpHeader.FromReader(reader);
                        if (!recConn.TryAcceptPaquet(header))
                            Debug.LogError($"{recConn} {nameof(recConn.TryAcceptPaquet)}: Failed to accept paquet (header:{header}, size:{reclength_u})");
                        else if (header.mask.HasFlag(RudpHeaderM.Eve | RudpHeaderM.Ack))
                            if (HasNext())
                            {
                                var publicEnd = reader.ReadIPEndPoint();
                                if (this.publicEnd == null || !this.publicEnd.Equals(publicEnd))
                                    Debug.Log($"{this} Received Public End: {publicEnd}");
                                this.publicEnd = publicEnd;

                                if (HasNext())
                                    if (eveClient == null)
                                        Debug.LogError($"{recConn} Received EVE paquet without {nameof(eveClient)} (header:{header}, size:{reclength_u})");
                                    else if (recConn != eveClient.eveConn)
                                        Debug.LogError($"{recConn} Received EVE paquet for wrong {nameof(eveClient)} (header:{header}, size:{reclength_u})");
                                    else
                                        while (HasNext())
                                            eveClient.OnEveAck();
                            }
                    }
                    else if (logEmptyPaquets)
                        Debug.Log($"{this} Received empty paquet from {remoteEnd}".ToSubLog());
                    else if (reclength_u > 0)
                        Debug.LogWarning($"{this} Received dubious paquet from {remoteEnd} (size:{reclength_u})");

                    recEnd_u = null;
                    recConn = null;
                }
            }
            catch (SocketException e)
            {
                lock (this)
                    if (skipNextSocketException)
                        skipNextSocketException = false;
                    else
                        Debug.LogWarning(e.Message());
            }
            catch (Exception e) { Debug.LogException(e); }
            BeginReceive();
        }

        void BeginReceive()
        {
            EndPoint receiveEnd = netpReceive;
            try { BeginReceiveFrom(PAQUET_BUFFER, 0, BUFFER_SIZE, SocketFlags.None, ref receiveEnd, ReceiveFrom, null); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}