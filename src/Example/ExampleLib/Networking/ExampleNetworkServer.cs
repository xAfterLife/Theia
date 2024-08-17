using System.Net.Sockets;
using NetCoreServer;

namespace ExampleLib.Networking;

public class ExampleNetworkServer(string address, int port, string handlerSpace) : TcpServer(address, port)
{
    protected override TcpSession CreateSession()
    {
        var session = new ExampleNetworkSession(this, handlerSpace);
        return session;
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