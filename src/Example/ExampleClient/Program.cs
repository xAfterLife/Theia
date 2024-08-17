using ExampleLib.Networking;

namespace ExampleClient;

internal static class Program
{
    private static async Task Main()
    {
        CancellationTokenSource cts = new();

        for ( var i = 0; i < 1_000; i++ )
            _ = Task.Run(() =>
            {
                ExampleNetworkClient client = new("127.0.0.1", 42069);
                client.Connect();

                ulong counter = 0;

                while ( true )
                {
                    client.SendPacket("msg", "0^pulse");
                    client.SendPacket("msg", $"0^{counter++}");
                }
            }, cts.Token);

        Thread.Sleep(10_000);
        await cts.CancelAsync();
    }
}