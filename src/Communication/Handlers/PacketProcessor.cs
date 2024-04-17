using Communication.Models;
using Communication.Packets;

namespace Communication.Handlers;

public class PacketProcessor(string handlerSpace, Dictionary<string, IPacketHandler> packetHandlers)
{
    public bool RegisterHandler(string header, IPacketHandler handler)
    {
        return packetHandlers.TryAdd(header, handler);
    }

    public Task HandlePacket(ISession session, Packet packet)
    {
        if ( packetHandlers.TryGetValue(packet.Header, out var packetHandler) )
            packetHandler.HandlePacket(session, ref packet);

        Console.WriteLine($"[PacketHandler - {handlerSpace}] No definition for PacketHeader {packet.Header} found");
        return Task.CompletedTask;
    }
}