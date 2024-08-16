using Communication.Handlers;
using Communication.Handlers.Attributes;
using Communication.Models;

namespace ExampleLib.Packets.MsgPacket;

[HandlerSpace("example")]
public class MsgPacketHandler : PacketHandler<MsgPacket>
{
    protected override Task HandlePacket(ISession session, MsgPacket packet)
    {
        //Console.WriteLine($"Received message type: {packet.MessageType}{Environment.NewLine}message: {packet.Message}");
        return Task.CompletedTask;
    }
}