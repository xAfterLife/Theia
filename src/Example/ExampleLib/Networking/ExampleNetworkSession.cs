﻿using System.Net.Sockets;
using Communication.Handlers;
using Communication.Models;
using Communication.Packets;
using Communication.Serialization;
using NetCoreServer;

namespace ExampleLib.Networking;

public sealed class ExampleNetworkSession(TcpServer server, string handlerSpace) : TcpSession(server), ISession
{
    private readonly PacketProcessor _packetProcessor = PacketProcessorFactory.CreateProcessor(handlerSpace);

    private byte[] _incompletePacketBuffer = [];

    private long SendPacket(Packet packet)
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
        foreach ( var packet in PacketSerializer.MessageToPackets(buffer[(int)offset..(int)size], ref _incompletePacketBuffer) )
        {
            var deserializePacket = PacketDeserializer.DeserializePacket(packet);

            if ( deserializePacket == null )
                Console.WriteLine($"[PacketHandler - {handlerSpace}] No definition for Header {packet.Header} found");
            else
                _packetProcessor.HandlePacket(this, deserializePacket);
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