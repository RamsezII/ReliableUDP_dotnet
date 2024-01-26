using _RUDP_;

internal class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("creating socket...");
        using var socket = new RudpSocket();
        Console.WriteLine($"Local IP: {socket.localIP}");

        return 0;
    }
}