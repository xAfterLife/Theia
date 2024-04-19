using System.Net.Sockets;
using Communication.Packets;
using Communication.Serialization;
using TcpClient = NetCoreServer.TcpClient;

namespace ExampleLib.Networking;

public class ExampleNetworkClient(string address, int port) : TcpClient(address, port)
{
    private bool _stop;

    public long SendPacket(Packet packet)
    {
        var buffer = PacketSerializer.SerializePacket(packet);
        return Send(buffer);
    }

    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while ( IsConnected )
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"Chat TCP client connected a new session with Id {Id}");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat TCP client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        Console.WriteLine("Trying to Reconnect..");
        if ( !_stop )
            ConnectAsync();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        //TODO: Implement ClientPacketHandler
        foreach ( var packet in PacketSerializer.DeserializePackets(buffer[(int)offset..(int)size]) )
            Console.WriteLine($"[ExampleClient - OnReceived] {packet}");
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat TCP client caught an error with code {error}");
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