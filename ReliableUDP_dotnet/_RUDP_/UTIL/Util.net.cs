using System.Net;

namespace _RUDP_
{
    public static partial class Util
    {
        public const byte VERSION = 0;
        public const string DOMAIN_3VE = "www.shitstorm.ovh";
        public const ushort PORT_ReliableUDP = 12345;
        public static readonly IPAddress IP_3VE = IPAddress.Parse("141.94.223.114");
        public static readonly IPEndPoint END_3VE = new(IP_3VE, PORT_ReliableUDP);
        public static readonly IPEndPoint END_LOOPBACK = new(IPAddress.Loopback, PORT_ReliableUDP);
    }
}