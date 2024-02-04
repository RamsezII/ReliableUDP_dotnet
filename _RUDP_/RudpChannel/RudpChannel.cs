using _UTIL_;
using System;
using System.IO;

namespace _RUDP_
{
    /// <summary>
    /// write, then read, then write, then read, etc... like a TCP stream
    /// </summary>
    public partial class RudpChannel : IDisposable
    {
        public readonly MemoryStream stream;
        public readonly BinaryReader reader;
        public readonly BinaryWriter writer;

        public readonly byte local_id;
        public byte remote_id;
        public readonly RudpConnection conn;
        public Action<bool> onSuccess;
        public Action<RudpChannel, BinaryReader> onDirectRead;

        public byte last_recID;
        public readonly ThreadSafe<bool> disposed = new();
        public override string ToString() => $"{conn} (channel:{local_id})";

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in RudpConnection conn, in byte local_id, in byte remote_id, in Action<bool> onSuccess)
        {
            stream = new();
            reader = new(stream);
            writer = new(stream);

            this.local_id = local_id;
            this.remote_id = remote_id;
            this.conn = conn;
            this.onSuccess = onSuccess;
        }

        //----------------------------------------------------------------------------------------------------------

        public bool ReadReady()
        {
            lock (stream)
                return stream.Remaining() > 0;
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;
            
            lock (pendingPaquets)
                if (pendingPaquets.Count > 0)
                {
                    foreach (RudpPaquet paquet in pendingPaquets)
                        paquet.Dispose();
                    lock (pendingPaquets)
                        pendingPaquets.Clear();
                }
        }
    }
}