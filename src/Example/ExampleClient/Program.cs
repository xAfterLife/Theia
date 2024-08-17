using ExampleLib.Networking;

namespace ExampleClient;

internal static class Program
{
    private static void Main()
    {
        List<ExampleNetworkClient> clients = [];

        for ( var i = 0; i < 10_000; i++ )
        {
            var client = new ExampleNetworkClient("127.0.0.1", 42069);
            clients.Add(client);
            client.Connect();
        }

        foreach ( var client in clients )
            client.SendPacket("msg", "0^This is a Test");
    }
}