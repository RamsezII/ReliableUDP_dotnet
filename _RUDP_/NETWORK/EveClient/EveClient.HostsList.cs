using _UTIL_;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _RUDP_
{
    public partial class EveClient
    {
        public readonly ThreadSafe<bool> refreshList = new();
        ushort listOffset;
        float lastListRequest;
        public readonly Dictionary<string, double> hosts = new(StringComparer.OrdinalIgnoreCase);
        public ThreadSafe_flag<string> hostsList = new();

        //----------------------------------------------------------------------------------------------------------

        void QueryList()
        {
            lastListRequest = Time.unscaledTime;
            eveConn.toRemote.WriteAndSend(writer =>
            {
                writer.Write((byte)EveCodes.ListHosts);
                lock (this)
                    writer.Write(listOffset);
            });
        }

        void OnListAck()
        {
            double time = Util.TotalMilliseconds;
            ushort totalCount;

            lock (eveConn.socket.stream)
            {
                totalCount = eveConn.socket.reader.ReadUInt16();
                while (eveConn.socket.HasNext())
                {
                    lock (this)
                        ++listOffset;
                    lock (hosts)
                        hosts[eveConn.socket.reader.ReadText()] = time;
                }
            }

            lock (this)
                if (listOffset >= totalCount)
                    listOffset = 0;

            HashSet<string> obseletes = null;
            lock (hosts)
                foreach (var pair in hosts)
                    if (time > pair.Value + 3000)
                        if (obseletes == null)
                            obseletes = new HashSet<string>() { pair.Key };
                        else
                            obseletes.Add(pair.Key);

            if (obseletes != null && obseletes.Count > 0)
                lock (hosts)
                    foreach (var key in obseletes)
                        hosts.Remove(key);

            BuildList();
        }

        void BuildList()
        {
            double time = Util.TotalMilliseconds;
            StringBuilder sb = new("{ ");
            lock (hosts)
                foreach (var pair in hosts)
                    sb.AppendLine($"\"{pair.Key.Bold()}\" ({(time - pair.Value).MillisecondsLog()}), ");
            hostsList.Value = sb.ToString()[..^1] + " }";
        }
    }
}