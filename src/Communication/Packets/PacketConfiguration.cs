using CommunityToolkit.HighPerformance.Buffers;

namespace Communication.Packets;

public static class PacketConfiguration
{
    public static readonly StringPool PacketPool = new();

    public static readonly char HeaderDelimiter = '|';
    public static readonly char ContentDelimiter = '^';
}