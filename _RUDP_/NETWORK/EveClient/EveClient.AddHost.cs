using _UTIL_;
using UnityEngine;

namespace _RUDP_
{
    public enum HostStates : byte
    {
        _none_,
        None,
        Adding,
        Hosting,
        _last_,
    }

    public partial class EveClient
    {
        public readonly ThreadSafe_flag<HostStates> hostState = new();
        public readonly ThreadSafe<string> hostName = new();
        public readonly ThreadSafe<int> publicHash = new();
        float lastAddRequest;
        void OnAddHostAck() => hostState.Value = eveConn.socket.reader.ReadBool() ? HostStates.Hosting : HostStates.None;

        //----------------------------------------------------------------------------------------------------------

        public void AddHost(string hostName, int publicHash)
        {
            this.hostName.Value = hostName;
            this.publicHash.Value = publicHash;

            if (hostState.Value != HostStates.Hosting)
                hostState.Value = HostStates.Adding;
        }

        void MaintainHost()
        {
            lastAddRequest = Time.unscaledTime;

            if (hostState.Value != HostStates.Hosting)
                hostState.Value = HostStates.Adding;

            eveConn.toRemote.WriteAndSend(writer =>
            {
                writer.Write((byte)EveCodes.AddHost);
                writer.Write(publicHash.Value);
                writer.WriteText(hostName.Value);
            });
        }
    }
}