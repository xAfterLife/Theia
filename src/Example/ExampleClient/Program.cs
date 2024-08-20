using ExampleLib.Networking;

namespace ExampleClient;

internal static class Program
{
    private static void Main()
    {
        var client = new ExampleNetworkClient("127.0.0.1", 42069);
        client.Connect();

        for ( var i = 0; i < 1_000; i++ )
        {
            client.SendPacket("msg", "0^Das ist ein Test");
            Thread.Sleep(1);
        }
    }
}