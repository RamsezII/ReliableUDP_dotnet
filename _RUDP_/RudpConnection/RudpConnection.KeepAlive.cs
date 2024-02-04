using System.Collections;

namespace _RUDP_
{
    public partial class RudpConnection
    {
        IEnumerator eKeepAlive;

        //----------------------------------------------------------------------------------------------------------

        public void ToggleKeepAlive(in bool enable)
        {
            lock (this)
                if (enable)
                    eKeepAlive ??= EKeepAliveLoop();
                else if (eKeepAlive != null)
                    eKeepAlive = null;
        }

        IEnumerator EKeepAliveLoop()
        {
            while (true)
            {
                double time = Util.TotalMilliseconds;
                if (time > lastSend + 5000)
                {
                    lock (this)
                        lastSend = time;
                    socket.SendEmptyTo(endPoint);
                }
                yield return null;
            }
        }
    }
}