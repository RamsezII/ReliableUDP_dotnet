using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        public static bool logUnexpectedAcks = true;

        //----------------------------------------------------------------------------------------------------------

        public bool TryAcceptAck(in RudpHeader header)
        {
            lock (pendingPaquets)
                if (pendingPaquets.TryPeek(out RudpPaquet paquet))
                    lock (paquet)
                    {
                        if (header.id == paquet.header.id)
                        {
                            pendingPaquets.Dequeue();
                            paquet.Dispose();
                            return true;
                        }
                        else if (logUnexpectedAcks)
                            Debug.LogWarning($"{this} Received ACK for unknown paquet:{header}");
                    }
                else if (logUnexpectedAcks)
                    Debug.LogWarning($"{this} Received unexpected ACK with paquet:{header}");
            return false;
        }
    }
}