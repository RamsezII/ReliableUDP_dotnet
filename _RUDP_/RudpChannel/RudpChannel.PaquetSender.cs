using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _RUDP_
{
    public partial class RudpChannel
    {
        public readonly Queue<RudpPaquet> pendingPaquets = new(50);
        byte last_sendID;

        //----------------------------------------------------------------------------------------------------------

        public void WriteAndSend(in Action<BinaryWriter> onWriter, bool push = true, in RudpHeaderM optionalMask = 0)
        {
            lock (stream)
            {
                writer.BeginWrite(out ushort prefixePos);
                onWriter?.Invoke(writer);
                writer.EndWriteRUDP(prefixePos);
                if (push)
                    PushAllData(optionalMask);
            }
        }

        public bool TrySendPeekPaquet()
        {
            lock (pendingPaquets)
                if (pendingPaquets.Count > 0 && pendingPaquets.TryPeek(out RudpPaquet paquet))
                    if (paquet.disposed.Value)
                    {
                        pendingPaquets.Dequeue();
                        if (pendingPaquets.Count > 0)
                            TrySendPeekPaquet();
                    }
                    else
                        lock (paquet)
                        {
                            if (paquet.attempt >= byte.MaxValue)
                                Debug.LogError($"{this} {nameof(TrySendPeekPaquet)} attempt overflow for paquet: {paquet}".ToSubLog());

                            double time = Util.TotalMilliseconds;
                            ushort delay = paquet.GetDelay;
                            if (time - paquet.lastAttempt < delay)
                                return false;
                            paquet.lastAttempt = time;

                            lock (paquet.buffer)
                            {
                                paquet.header.WriteToBuffer(paquet.buffer);
                                paquet.buffer[(int)RudpHeaderI.Attempt] = paquet.attempt;

                                conn.Send(paquet.buffer);

                                if (paquet.attempt < byte.MaxValue)
                                    ++paquet.attempt;
                            }

                            return true;
                        }
            return false;
        }
    }
}