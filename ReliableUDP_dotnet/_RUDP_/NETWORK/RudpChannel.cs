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
        public readonly byte id;
        public readonly RudpConnection conn;
        public bool readAvailable;

        //----------------------------------------------------------------------------------------------------------

        public RudpChannel(in byte id, in RudpConnection conn)
        {
            this.id = id;
            this.conn = conn;
            stream = new();
            reader = new(stream);
            writer = new(stream);
        }

        public override string ToString() => $"{conn} (channel:{id})";

        //----------------------------------------------------------------------------------------------------------

        bool TryStartNewPaquet(out RudpHeader header)
        {
            header = new(RudpHeaderM.Reliable, conn.GetIncrementedSendID(), id);
            RudpSocket.stream.Position = 0;
            RudpSocket.writer.WriteRudpHeader(header);
            return true;
        }

        public void SendReliable(in bool loop = true)
        {
            Console.WriteLine($"{this} Sending...");
            reader.BaseStream.Position = 0;

            lock (RudpSocket.BUFFER)
            {
                TryStartNewPaquet(out RudpHeader header);
                while (reader.BaseStream.Remaining() > 0)
                {
                    ushort length = reader.ReadUInt16();
                    if (length > 0)
                    {
                        if (length > RudpSocket.stream.Remaining())
                        {
                            conn.socket.SendTo(conn.remoteEnd);
                            TryStartNewPaquet(out header);
                        }
                        RudpSocket.writer.Write(length);
                        reader.BaseStream.CopyTo(RudpSocket.stream, length);
                    }
                }
                if (RudpSocket.stream.Position > 0)
                    conn.socket.SendTo(conn.remoteEnd);
            }

            if (reader.BaseStream.Remaining() == 0)
            {
                reader.BaseStream.Position = 0;
                reader.BaseStream.SetLength(0);
            }
            Console.WriteLine($"{this} Finished Sending");
        }

        public ushort TriggerLengthBasedBlock()
        {
            Console.WriteLine($"{this} Start Blockread... ({Thread.CurrentThread.Name})");
            while (stream.Remaining() < sizeof(ushort))
            {
                Console.WriteLine($"{this} Into ReadBlock");
                Console.WriteLine($"{this} Out from ReadBlock");
            }

            ushort length;
            lock (stream)
                length = reader.ReadUInt16();

            TriggerLengthBasedBlock(length);

            return length;
        }

        public void TriggerLengthBasedBlock(ushort length)
        {
            Console.WriteLine($"{this} Blockread {length} bytes...  ({Thread.CurrentThread.Name})");
            while (stream.Remaining() < length)
            {
                Console.WriteLine($"{this} Into ReadBlock");
                readAvailable.Reset();
                readAvailable.Wait();
                Console.WriteLine($"{this} Out from ReadBlock");
            }
            Console.WriteLine($"{this} Finished Blockread  ({Thread.CurrentThread.Name})");
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            conn.channels.Remove(id);
        }
    }
}