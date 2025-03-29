using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Proxy;

public class HttpWorker(MyWebServer webServer) : BackgroundService
{
    protected override void ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            webServer.Start();
            Debug.WriteLine("HTTP worker started");
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
}