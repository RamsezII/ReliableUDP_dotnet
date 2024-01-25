namespace _RUDP_
{
    public class DoubleStream : IDisposable
    {
        public abstract class Borrow : IDisposable
        {
            public readonly DoubleStream stream;
            public Borrow(in DoubleStream stream) => this.stream = stream;
            public abstract void Dispose();
        }

        public class BorrowWriter : Borrow
        {
            public readonly BinaryWriter writer;
            public BorrowWriter(in DoubleStream stream) : base(stream)
            {
                stream.stream.Position = stream.i_write;
                writer = stream.writer;
            }
            public override void Dispose() => stream.i_write = (ushort)stream.stream.Position;
        }

        public class BorrowReader : Borrow
        {
            public readonly BinaryReader reader;
            public BorrowReader(in DoubleStream stream) : base(stream)
            {
                stream.stream.Position = stream.i_read;
                reader = stream.reader;
            }
            public override void Dispose() => stream.i_read = (ushort)stream.stream.Position;
        }

        public ushort i_read, i_write;
        public readonly MemoryStream stream;
        readonly BinaryWriter writer;
        readonly BinaryReader reader;

        public BorrowWriter DisposableWrite() => new(this);
        public BorrowReader DisposableRead() => new(this);

        //----------------------------------------------------------------------------------------------------------

        public DoubleStream() : this(new MemoryStream())
        {
        }

        public DoubleStream(in ushort size) : this(new MemoryStream(size))
        {
        }

        public DoubleStream(in byte[] buffer) : this(new MemoryStream(buffer))
        {
        }

        public DoubleStream(in MemoryStream stream)
        {
            this.stream = stream;
            writer = new(stream);
            reader = new(stream);
        }

        //----------------------------------------------------------------------------------------------------------

        public void Reset()
        {

        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            writer.Dispose();
            reader.Dispose();
            stream.Dispose();
        }
    }
}