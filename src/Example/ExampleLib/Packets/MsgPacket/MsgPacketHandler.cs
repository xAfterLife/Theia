using Communication.Handlers;
using Communication.Models;
using Communication.Packets;
using Communication.Packets.Attributes;

namespace ExampleLib.Packets.MsgPacket;

[PacketDefinition("msg", "example")]
public class MsgPacketHandler : IPacketHandler
{
    public Task HandlePacket(ISession session, ref Packet packet)
    {
        Console.WriteLine($"Received message: {packet.FullPacket}");
        return Task.CompletedTask;
    }
}