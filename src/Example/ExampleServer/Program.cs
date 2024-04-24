using ExampleLib.Networking;
using ExampleLib.Packets.MsgPacket;

namespace ExampleServer;

internal class Program
{
    private static void Main(string[] args)
    {
        ExampleNetworkServer server = new("127.0.0.1", 42069, "example");
        server.Start();

        while ( true )
        {
            MsgPacket packet = new(0, "test");
            var message = Console.ReadLine();
            if ( message == "quit" )
                Environment.Exit(0);
        }
    }
}