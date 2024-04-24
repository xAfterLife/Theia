using Communication.Models;
using Communication.Packets;

namespace Communication.Handlers;

public interface IPacketHandler
{
    public Task HandlePacket(ISession session, IPacket packet);
}