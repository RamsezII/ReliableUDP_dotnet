using System.Net;

namespace _RUDP_
{
    public partial class RudpSocket
    {
        public readonly Dictionary<IPEndPoint, RudpConnection> connections = [];

        //----------------------------------------------------------------------------------------------------------

        public bool TryConnect(in IPEndPoint remoteEnd, out RudpConnection? conn)
        {
            if (ToConnection(remoteEnd, out conn))
                if (conn.TryAddNewChannel(out RudpChannel? channel))
                {
                    channel.writer.BeginWrite(out ushort prefixePos);
                    channel.writer.WriteStr("JE ME CONNÈCQUE");
                    channel.writer.EndWrite(prefixePos);
                    channel.SendReliable();
                    channel.TriggerLengthBasedBlock();
                    Console.WriteLine(channel.reader.ReadStr());
                    return true;
                }
            return false;
        }
    }
}