using _UTIL_;
using System;
using UnityEngine;

namespace _RUDP_
{
    public enum EveCodes : byte
    {
        _none_,
        GetPublicEnd,
        ListHosts,
        AddHost,
        JoinHost,
        _last_,
    }

    public partial class EveClient : IDisposable
    {
        public readonly RudpConnection eveConn;
        public EveCodes code;
        public readonly ThreadSafe<bool> disposed = new();

        //----------------------------------------------------------------------------------------------------------

        public EveClient(in RudpConnection eveConn)
        {
            this.eveConn = eveConn;
            eveConn.socket.publicEnd = null;
            BuildList();
            eveConn.toRemote.WriteAndSend(writer => writer.Write((byte)EveCodes.GetPublicEnd));
        }

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;
            eveConn.Dispose();
        }

        //----------------------------------------------------------------------------------------------------------

        public void Update()
        {
            if (Time.unscaledTime > lastListRequest + .5f)
                if (refreshList.Value)
                    QueryList();

            if (Time.unscaledTime > lastAddRequest + 2)
                switch (hostState.Value)
                {
                    case HostStates.Adding:
                    case HostStates.Hosting:
                        MaintainHost();
                        break;
                }
        }

        public void OnEveAck()
        {
            if (disposed.Value)
                return;

            EveCodes code = (EveCodes)eveConn.socket.reader.ReadByte();
            switch (code)
            {
                case EveCodes.GetPublicEnd:
                    refreshList.Value = true;
                    break;

                case EveCodes.ListHosts:
                    OnListAck();
                    break;

                    case EveCodes.AddHost:
                    OnAddHostAck();
                    break;
            }
        }
    }
}