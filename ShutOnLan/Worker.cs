using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ShutOnLan;

public class Worker : BackgroundService
{
    private HttpListener? _listener;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://+:2381/");
        _listener.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            var context = await _listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            if (request.Url?.AbsolutePath == "/shutdown")
            {
                var buffer = Encoding.UTF8.GetBytes("Shutting down...");
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();

                _ = Task.Run(() => _shutdown());
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }
    }

    private void _shutdown()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            System.Diagnostics.Process.Start("shutdown", "/s /t 0");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            System.Diagnostics.Process.Start("shutdown", "-h now");
        }
        else
        {
            Console.WriteLine("Unsupported OS");
        }
    }

    public override void Dispose()
    {
        _listener?.Stop();
        _listener?.Close();
        base.Dispose();
    }
}