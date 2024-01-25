using scripts;

internal class Program
{

    //----------------------------------------------------------------------------------------------------------

    private static void Main(string[] args)
    {
        Console.WriteLine("creating socket...");
        using var socket = new NetSocketUDP();
        Console.WriteLine($"Local IP: {socket.localIP}");
    }
}