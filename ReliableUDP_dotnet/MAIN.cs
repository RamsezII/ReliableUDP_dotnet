using _RUDP_;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("creating socket...");
        using var socket = new UdpSocket();
        Console.WriteLine($"Local IP: {socket.localIP}");
    }
}