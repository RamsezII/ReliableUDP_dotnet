namespace _RUDP_
{
    public partial class RudpChannel
    {
        public readonly ManualResetEvent readAvailable = new(false);

        //----------------------------------------------------------------------------------------------------------

        public ushort TriggerLengthBasedBlock()
        {
            lock (stream)
                while (stream.Remaining() < sizeof(ushort))
                {
                    readAvailable.Reset();
                    readAvailable.WaitOne();
                }

            ushort length;
            lock (stream)
                length = reader.ReadUInt16();

            TriggerLengthBasedBlock(length);

            return length;
        }

        public void TriggerLengthBasedBlock(ushort length)
        {
            lock (stream)
                while (stream.Remaining() < length)
                {
                    readAvailable.Reset();
                    readAvailable.WaitOne();
                }
        }
    }
}