using System.Net.Sockets;
using Communication.Handlers;
using Communication.Models;
using Communication.Packets;
using Communication.Serialization;
using NetCoreServer;

namespace ExampleLib.Networking;

public sealed class ExampleNetworkSession(TcpServer server, string handlerSpace) : TcpSession(server), ISession
{
    private readonly PacketProcessor _packetProcessor = PacketProcessorFactory.CreateProcessor(handlerSpace);

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
        foreach ( var packet in PacketSerializer.DeserializePackets(buffer[(int)offset..(int)size]) )
            _packetProcessor.HandlePacket(this, packet);
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