namespace ShutOnLan;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            })
            .UseWindowsService()
            .Build();

        await host.RunAsync();
    }
}