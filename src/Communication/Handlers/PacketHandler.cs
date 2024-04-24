using Communication.Models;
using Communication.Packets;

namespace Communication.Handlers;

public abstract class PacketHandler<T> : IPacketHandler where T : IPacket
{
    public Task HandlePacket(ISession session, IPacket packet)
    {
        // Check if the packet is of the correct type
        if ( packet is T typedPacket )
            // Call the abstract method to handle the packet
            return HandlePacket(session, typedPacket);
        // Handle the case when the packet is not of the expected type
        // For example, throw an exception or return a completed task
        return Task.CompletedTask;
    }

    protected abstract Task HandlePacket(ISession session, T packet);
}