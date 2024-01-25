using _RUDP_;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("creating socket...");
        using var socket = new RudpSocket();
        Console.WriteLine($"Local IP: {RudpSocket.localIP}");
    }
}