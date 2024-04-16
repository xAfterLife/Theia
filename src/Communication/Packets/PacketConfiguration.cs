namespace Communication.Packets;

internal static class PacketConfiguration
{
    public static char HeaderSplitter = '|';
    public static char ContentSplitter = '^';
    public static char EndOfPacket = (char)0xFF;

    // Example Packet: 'msg|Hello World^This is a message'
}