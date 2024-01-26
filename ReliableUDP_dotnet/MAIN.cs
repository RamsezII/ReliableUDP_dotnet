using _RUDP_;

internal class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("creating socket A...");
        using var A = new RudpSocket();
        Console.WriteLine($"portA: {A.localEndIP.Port}");

        Console.WriteLine("creating socket B...");
        using var B = new RudpSocket();
        Console.WriteLine($"portB: {B.localEndIP.Port}");

        A.ToConnection(B.localEndIP, out var AtoB);
        B.ToConnection(A.localEndIP, out var BtoA);

        return 0;
    }
}