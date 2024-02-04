using _UTIL_;
using System;

namespace _RUDP_
{
    public class RudpPaquet : IDisposable
    {
        public byte attempt;
        public double lastAttempt;
        public readonly RudpHeader header;
        public byte[] buffer;
        public readonly ThreadSafe<bool> disposed = new();
        public readonly Action onAck, onFailure;
        public override string ToString() => $"{header} (attempt:{attempt})";

        //----------------------------------------------------------------------------------------------------------

        public RudpPaquet(in RudpHeader header, in byte[] buffer)
        {
            this.header = header;
            this.buffer = buffer;
            header.WriteToBuffer(buffer);
        }

        //----------------------------------------------------------------------------------------------------------

        public ushort GetDelay => attempt switch
        {
            0 => 0,
            1 => 100,
            2 => 150,
            3 => 300,
            4 => 600,
            _ => 900,
        };

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;
            buffer = null;
        }
    }
}