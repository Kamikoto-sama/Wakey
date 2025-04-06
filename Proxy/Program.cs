using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using nanoFramework.Runtime.Native;

namespace Proxy;

public class Program
{
    private static readonly CancellationTokenSource ShutdownTokenSource = new();

    public static void Main()
    {
        try
        {
            Debug.WriteLine("Hello from nanoFramework!");
            WifiHelper.ConnectToWifi();

            var builder = new HostBuilder();
            builder.ConfigureServices(ConfigureServices);

            using var host = builder.Build();

            var webServer = (MyWebServer)host.Services.GetRequiredService(typeof(MyWebServer));
            webServer.Start();
            Debug.WriteLine("HTTP server started");

            host.StartAsync(ShutdownTokenSource.Token);
            ShutdownTokenSource.Token.WaitHandle.WaitOne();
            Debug.WriteLine("Shutdown token was cancelled");

            webServer.Stop();
            host.StopAsync();
            ShutdownTokenSource.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Format());
        }

        Power.RebootDevice();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(typeof(CancellationTokenSource), ShutdownTokenSource);
        services.AddSingleton(typeof(Ping));
        services.AddSingleton(typeof(State));
        services.AddSingleton(typeof(WakeOnLan));
        services.AddSingleton(typeof(MyWebServer));
        services.AddSingleton(typeof(ApiConnection));
        services.AddHostedService(typeof(StateWorker));
    }
}