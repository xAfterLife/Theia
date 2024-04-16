using MessagePack;

namespace Communication.Packets;

[MessagePackObject]
public class Packet
{
    [IgnoreMember] public string Header;

    [IgnoreMember] public string Content;

    [Key(0)] public string FullPacket;

    [Key(1)] public char PacketSplitter;

    [SerializationConstructor]
    public Packet(string fullPacket)
    {
        FullPacket = fullPacket;
        PacketSplitter = PacketConfiguration.EndOfPacket;

        var temp = fullPacket.Split(PacketConfiguration.HeaderSplitter);
        Header = temp[0];
        Content = temp[1];
    }
}