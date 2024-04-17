using System.Net.Sockets;
using NetCoreServer;

namespace Communication.Networking;

public class NetworkServer(string address, int port, string handlerSpace) : TcpServer(address, port)
{
    public string HandlerSpace { get; init; } = handlerSpace;

    protected override TcpSession CreateSession()
    {
        return new NetworkSession(this);
    }

    protected override void OnStarted()
    {
        Console.WriteLine($"TCP server listening on {Endpoint}!");
    }

    protected override void OnStopped()
    {
        Console.WriteLine("TCP server stopped!");
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat TCP server caught an error with code {error}");
    }
}