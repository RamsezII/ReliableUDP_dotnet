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

        socket_A.ToConnection(socket_B.localEndIP, out var conn_AtoB);
        socket_B.ToConnection(socket_A.localEndIP, out var conn_BtoA);

        conn_AtoB.stdin.writer.BeginWrite(out ushort prefPos);
        conn_AtoB.stdin.writer.WriteStr("hello from A");
        conn_AtoB.stdin.writer.EndWrite(prefPos);
        conn_AtoB.stdin.SendReliable();

        conn_BtoA.stdout.TriggerLengthBasedBlock();
        Console.WriteLine(conn_BtoA.stdout.reader.ReadStr());

        return 0;
    }
}