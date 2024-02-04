using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public readonly Dictionary<IPEndPoint, RudpPuncher> punchers = new();
        public readonly Dictionary<IPEndPoint, RudpConnection> connections = new();
        public EveClient eveClient;

        //----------------------------------------------------------------------------------------------------------

        public void StartEveClient(in IPEndPoint eveEnd) => eveClient ??= new EveClient(ToConnection(eveEnd));

        public void TryPunchToConnect(in IPEndPoint targetEnd, in Action<RudpConnection> onConnection, in Action onFailure)
        {
            lock (punchers)
                if (punchers.TryGetValue(targetEnd, out RudpPuncher connector))
                    Debug.LogError($"{this} {nameof(TryPunchToConnect)}: Already connecting to {{ {targetEnd} }}");
                else
                    punchers[targetEnd] = new RudpPuncher(this, targetEnd, onConnection, onFailure);
        }

        public RudpConnection ToConnection(in IPEndPoint remoteEnd) => ToConnection(remoteEnd, out _);
        public RudpConnection ToConnection(in IPEndPoint remoteEnd, out bool isnew)
        {
            if (connections.TryGetValue(remoteEnd, out RudpConnection conn))
                isnew = false;
            else
            {
                conn = new RudpConnection(this, remoteEnd);
                lock (connections)
                    connections[remoteEnd] = conn;

                if (remoteEnd.Address.Equals(IPAddress.Loopback))
                    lock (connections)
                        connections[new IPEndPoint(IPAddress.Any, remoteEnd.Port)] = conn;

                if (remoteEnd.Address.Equals(IPAddress.Any))
                    lock (connections)
                        connections[new IPEndPoint(IPAddress.Loopback, remoteEnd.Port)] = conn;

                isnew = true;
            }
            return conn;
        }
    }
}