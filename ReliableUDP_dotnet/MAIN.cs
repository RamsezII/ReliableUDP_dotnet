using System.Net;
using _RUDP_;

internal class Program
{
    private static int Main(string[] args)
    {
        Thread.CurrentThread.Name = "MAIN";

        Console.WriteLine("creating socket A...");
        using var socket_A = new RudpSocket();
        Console.WriteLine($"portA: {socket_A.localEndIP.Port}");

        Console.WriteLine("creating socket B...");
        using var socket_B = new RudpSocket();
        Console.WriteLine($"portB: {socket_B.localEndIP.Port}");

        while (true)
        {
            socket_A.ToConnection(new IPEndPoint(IPAddress.Loopback, socket_B.localEndIP.Port), out var conn_AtoB);

            Console.Write("AtoB:~$ ");
            conn_AtoB.stdin.writer.BeginWrite(out ushort prefPos);
            conn_AtoB.stdin.writer.WriteStr(Console.ReadLine() ?? "");
            conn_AtoB.stdin.writer.EndWrite(prefPos);
            conn_AtoB.stdin.SendReliable();

            socket_B.ToConnection(new IPEndPoint(IPAddress.Loopback, socket_A.localEndIP.Port), out var conn_BtoA);
            conn_BtoA.stdout.TriggerLengthBasedBlock();
            Console.WriteLine("B receive A: " + conn_BtoA.stdout.reader.ReadStr());

            Console.Write("BtoA:~$ ");
            conn_BtoA.stdin.writer.BeginWrite(out prefPos);
            conn_BtoA.stdin.writer.WriteStr(Console.ReadLine() ?? "");
            conn_BtoA.stdin.writer.EndWrite(prefPos);
            conn_BtoA.stdin.SendReliable();

            conn_AtoB.stdout.TriggerLengthBasedBlock();
            Console.WriteLine("A receive B: " + conn_AtoB.stdout.reader.ReadStr());

            break;
        }
        return 0;
    }
}