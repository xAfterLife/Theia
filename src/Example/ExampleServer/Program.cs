using ExampleLib.Networking;

namespace ExampleServer;

internal static class Program
{
    private static void Main()
    {
        ExampleNetworkServer server = new("127.0.0.1", 42069, "example");
        server.Start();

        while ( true )
        {
            var message = Console.ReadLine();
            if ( message == "quit" )
                Environment.Exit(0);
        }
    }
}