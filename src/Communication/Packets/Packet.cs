using MessagePack;

namespace Communication.Packets;

[MessagePackObject]
public readonly struct Packet
{
    [Key(0)]
    public string Header { get; }

    [Key(1)]
    public string Content { get; }

    [IgnoreMember]
    public string FullPacket => $"{Header}{PacketConfiguration.HeaderDelimiter}{Content}";

    [SerializationConstructor]
    public Packet(string header, string content)
    {
        Header = PacketConfiguration.PacketPool.GetOrAdd(header);
        Content = PacketConfiguration.PacketPool.GetOrAdd(content);
    }

    public Packet(ReadOnlySpan<char> header, ReadOnlySpan<char> content)
    {
        Header = PacketConfiguration.PacketPool.GetOrAdd(header);
        Content = PacketConfiguration.PacketPool.GetOrAdd(content);
    }

    public Packet(ReadOnlySpan<char> packet)
    {
        var splitterIndex = packet.IndexOf(PacketConfiguration.HeaderDelimiter);
        Header = PacketConfiguration.PacketPool.GetOrAdd(packet[..splitterIndex]);
        Content = PacketConfiguration.PacketPool.GetOrAdd(packet[(splitterIndex + 1)..]);
    }

    public override string ToString()
    {
        return FullPacket;
    }
}