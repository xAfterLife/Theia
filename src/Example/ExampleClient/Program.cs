using ExampleLib.Networking;

namespace ExampleClient;

internal class Program
{
    private static void Main(string[] args)
    {
        ExampleNetworkClient client = new("127.0.0.1", 42069);
        client.Connect();

        while ( true )
        {
            var message = Console.ReadLine();
            if ( string.IsNullOrEmpty(message) )
                break;

            client.SendPacket("msg", $"0^{message}");
        }
    }
}