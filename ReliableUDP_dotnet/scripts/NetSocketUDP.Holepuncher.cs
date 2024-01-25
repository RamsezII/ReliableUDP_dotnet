using System.Net;
using System.Net.Sockets;

namespace scripts
{
    public partial class NetSocketUDP
    {
        public class Holepuncher : IDisposable
        {
            public readonly byte id;
            public readonly NetSocketUDP socket;
            public readonly IPEndPoint remoteEnd;
            bool success, disposed, timedout;

            //----------------------------------------------------------------------------------------------------------

            public Holepuncher(in byte id, in NetSocketUDP socket, in IPEndPoint remoteEnd)
            {
                this.id = id;
                this.socket = socket;
                this.remoteEnd = remoteEnd;
            }

            //----------------------------------------------------------------------------------------------------------

            public async void Start()
            {
                while (!disposed && !success)
                {
                    socket.SendTo(Util.EMPTY_BUFFER, 0, 0, SocketFlags.None, remoteEnd);
                    await Task.Delay(delai);
                }
            }

            //----------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                disposed = true;
                socket.holepunchers.Remove(id);
            }
        }

        public readonly Dictionary<byte, Holepuncher> holepunchers = new();
    }
}