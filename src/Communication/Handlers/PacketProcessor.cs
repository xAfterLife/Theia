using Communication.Models;
using Communication.Packets;

namespace Communication.Handlers;

public class PacketProcessor(string handlerSpace, Dictionary<Type, IPacketHandler> packetHandlers)
{
    public bool RegisterHandler<T>(PacketHandler<T> handler) where T : IPacket
    {
        return packetHandlers.TryAdd(typeof(T), handler);
    }

    public void HandlePacket<T>(ISession session, T packet) where T : IPacket
    {
        var packetType = packet.GetType();
        if ( packetHandlers.TryGetValue(packetType, out var packetHandler) )
            packetHandler.HandlePacket(session, packet);
        else
            Console.WriteLine($"[PacketHandler - {handlerSpace}] No definition for PacketType {packetType} found");
    }
}