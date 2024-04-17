using System.Net.Sockets;
using Communication.Packets;
using Communication.Serialization;
using NetCoreServer;

namespace Communication.Networking;

public sealed class NetworkSession(TcpServer server) : TcpSession(server)
{
    public long SendPacket(Packet packet)
    {
        var buffer = PacketSerializer.SerializePacket(packet);
        return Send(buffer);
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"TCP session with Id {Id} connected!");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"TCP session with Id {Id} disconnected!");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        //TODO: Implement
        foreach ( var packet in PacketSerializer.DeserializePackets(buffer[(int)offset..(int)size]) )
        {
            //PacketProcessor.HandlePacket(this, ref packet);
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat TCP session caught an error with code {error}");
    }

    public long SendPacket(ReadOnlySpan<char> packet)
    {
        return SendPacket(new Packet(packet));
    }

    public long SendPacket(string header, string data)
    {
        return SendPacket(new Packet(header, data));
    }
}