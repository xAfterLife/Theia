using CommunityToolkit.HighPerformance.Buffers;

namespace Communication.Packets;

public static class PacketConfiguration
{
    public const char HeaderDelimiter = '|';
    public const char ContentDelimiter = '^';
    public static readonly StringPool PacketPool = new();
}