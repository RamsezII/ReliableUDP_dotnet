using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public void Update()
        {
            if (disposed.Value)
            {
                Debug.LogWarning($"{this} {nameof(Update)}: Disposed socket");
                return;
            }

            eveClient?.Update();

            if (punchers.Count > 0)
            {
                HashSet<IPEndPoint> removeKeys = null;

                foreach (RudpPuncher connector in punchers.Values)
                    if (connector.disposed.Value)
                        if (removeKeys == null)
                            removeKeys = new HashSet<IPEndPoint> { connector.remoteEnd };
                        else
                            removeKeys.Add(connector.remoteEnd);
                    else
                        connector.Update();

                if (removeKeys != null)
                    foreach (IPEndPoint connector in removeKeys)
                        lock (punchers)
                            punchers.Remove(connector);
            }

            if (connections.Count > 0)
                foreach (RudpConnection conn in connections.Values)
                    conn.OnNetworkPush();
        }
    }
}