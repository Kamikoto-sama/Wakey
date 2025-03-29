using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Proxy;

public class Program
{
    public static void Main()
    {
        try
        {
            Debug.WriteLine("Hello from nanoFramework!");
            WifiHelper.ConnectToWifi();

            var builder = new HostBuilder();
            builder.ConfigureServices(ConfigureServices);

            using var host = builder.Build();
            host.Run();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            throw;
        }
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(typeof(Ping));
        services.AddSingleton(typeof(State));
        services.AddSingleton(typeof(WakeOnLan));
        services.AddSingleton(typeof(MyWebServer));
        services.AddSingleton(typeof(ApiConnection));
        services.AddHostedService(typeof(HttpWorker));
        services.AddHostedService(typeof(StateWorker));
    }
}