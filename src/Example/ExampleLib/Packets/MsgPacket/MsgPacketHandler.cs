using Communication.Handlers;
using Communication.Models;

namespace ExampleLib.Packets.MsgPacket;

[HandlerSpace("example")]
public class MsgPacketHandler : PacketHandler<MsgPacket>
{
    public override Task HandlePacket(ISession session, MsgPacket packet)
    {
        Console.WriteLine($"Received message type: {packet.MessageType}{Environment.NewLine}message: {packet.Message}");
        return Task.CompletedTask;
    }
}