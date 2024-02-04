using _UTIL_;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _RUDP_
{
    public class RudpPuncher : IDisposable
    {
        public readonly RudpSocket socket;
        public readonly IPEndPoint remoteEnd;
        public Action<RudpConnection> onSuccess;
        Action onFailure;

        public readonly string log;

        public readonly ThreadSafe<bool> disposed = new();

        public IEnumerator routine;

        //----------------------------------------------------------------------------------------------------------

        public RudpPuncher(RudpSocket socket, IPEndPoint remoteEnd, in Action<RudpConnection> onSuccess, in Action onFailure, float timeout = 2, byte maxAttemps = 10)
        {
            this.socket = socket;
            this.remoteEnd = remoteEnd;
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;

            log = $"{{ {socket.netpLoopback} }} -> {{ {remoteEnd} }}";
            Debug.Log("RudpConnector: " + log);

            routine = EPunchLoop();
            IEnumerator EPunchLoop()
            {
                float startTime = Time.unscaledTime;
                byte attempts = 0;
                float lastSend = 0;

                while (startTime + timeout > Time.unscaledTime)
                {
                    if (disposed.Value)
                        yield break;

                    if (++attempts >= maxAttemps)
                    {
                        Debug.LogWarning($"Connection fail: {log} \"{maxAttemps} failed attempts\"");
                        yield break;
                    }

                    float delay = attempts switch
                    {
                        0 => 0,
                        1 => .05f,
                        2 => .1f,
                        3 => .2f,
                        4 => .5f,
                        _ => .85f,
                    };

                    while (Time.unscaledTime < lastSend + delay)
                        yield return null;
                    lastSend = Time.unscaledTime;

                    Debug.Log($"Connection attempt: {log} {attempts}");
                    try
                    {
                        socket.SendEmptyTo(remoteEnd);
                    }
                    catch (SocketException e)
                    {
                        Debug.LogError($"SocketException ({e.SocketErrorCode}): {remoteEnd} {log} {attempts}");
                        yield break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        yield break;
                    }
                }

                Debug.LogWarning($"Connection timeout: {log} \"{(Time.unscaledTime - startTime).TimeLog()} seconds\" ({timeout.TimeLog()})");
            }
        }

        //----------------------------------------------------------------------------------------------------------

        public void Update()
        {
            if (routine != null && !routine.MoveNext())
                Dispose();
        }

        //----------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            if (disposed.Value)
                return;
            disposed.Value = true;

            lock (this)
            {
                if (routine != null)
                    onFailure?.Invoke();

                routine = null;
                onSuccess = null;
                onFailure = null;
            }
        }
    }
}