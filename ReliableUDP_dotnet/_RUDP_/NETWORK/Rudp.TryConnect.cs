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
                    lock (channel.stream)
                    {
                        channel.writer.BeginWrite(out ushort prefixePos);
                        channel.writer.WriteRudpHeader(channel.GetHeader(RudpHeaderM.OpenReliable));
                        channel.writer.WriteStr("je tentoie de me connecter");
                        channel.writer.EndWrite(prefixePos);
                        channel.Send();
                    }

                    lock (channel.stream)
                    {
                        channel.TriggerLengthBasedBlock();
                        Console.WriteLine(channel.reader.ReadStr());
                        return true;
                    }
                }
            return false;
        }
    }
}