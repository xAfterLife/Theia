using Communication.Packets;
using Communication.Packets.Attributes;

namespace ExampleLib.Packets.MsgPacket;

[PacketDefinition("msg", "example")]
public class MsgPacket(int messageType, string message) : IPacket
{
    [PacketIndex(0)]
    public int MessageType { get; set; } = messageType;

    [PacketIndex(1)]
    public string Message { get; set; } = message;
}