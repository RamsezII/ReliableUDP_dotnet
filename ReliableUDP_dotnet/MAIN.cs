using _RUDP_;

internal class Program
{
    private static void Main(string[] args)
    {
        RudpHeaderM mask = RudpHeaderM.Unreliable;
        Console.WriteLine($"mask: {mask}");
        Console.WriteLine($"mask.HasFlag(RudpHeaderM.Unreliable): {mask.HasFlag(RudpHeaderM.Unreliable)}");
        Console.WriteLine("creating socket...");
        using var socket = new RudpSocket();
        Console.WriteLine($"Local IP: {socket.localIP}");
    }
}