using MemoryPack;

namespace Communication.Packets;

[MemoryPackable]
public readonly partial struct Packet
{
    [MemoryPackInclude]
    public string Header { get; }

    [MemoryPackInclude]
    public string Content { get; }

    [MemoryPackIgnore]
    public string FullPacket => $"{Header}{PacketConfiguration.HeaderDelimiter}{Content}";

    [MemoryPackConstructor]
    public Packet(string header, string content)
    {
        Header = header;
        Content = content;
    }

    public Packet(ReadOnlySpan<char> header, ReadOnlySpan<char> content)
    {
        Header = new string(header);
        Content = new string(content);
    }

    public Packet(ReadOnlySpan<char> packet)
    {
        var splitterIndex = packet.IndexOf(PacketConfiguration.HeaderDelimiter);
        Header = new string(packet[..splitterIndex]);
        Content = new string(packet[(splitterIndex + 1)..]);
    }

    public override string ToString()
    {
        return FullPacket;
    }
}