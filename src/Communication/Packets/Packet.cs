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