namespace _RUDP_
{
    public partial class RudpChannel
    {
        public void PushAllData(in RudpHeaderM optionalMask = 0)
        {
            lock (stream)
            {
                stream.Position = 0;
                while (stream.HasNext())
                {
                    ushort length = (ushort)(RudpHeader.RELIABLE_SIZE + reader.PredictDataSize_unprefixed(RudpSocket.DATA_SIZE));
                    byte[] buffer = new byte[length];
                    reader.CopyData_unprefixed(buffer, RudpHeader.RELIABLE_SIZE);

                    lock (this)
                    {
                        last_sendID = ++last_sendID == 0 ? (byte)1 : last_sendID;
                        RudpHeader header = new(RudpHeaderM.Reliable | optionalMask, remote_id, last_sendID);
                        RudpPaquet paquet = new(header, buffer);
                        lock (pendingPaquets)
                            pendingPaquets.Enqueue(paquet);
                    }
                }
                stream.Position = 0;
                stream.SetLength(0);
            }
        }
    }
}