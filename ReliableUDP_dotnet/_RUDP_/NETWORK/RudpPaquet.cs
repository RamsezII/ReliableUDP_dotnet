namespace _RUDP_
{
    public class RudpPaquet(in byte id, in byte[] buffer)
    {
        public readonly byte id = id;
        public readonly byte[] buffer = buffer;
    }
}