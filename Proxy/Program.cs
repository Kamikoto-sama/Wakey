using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using nanoFramework.Json;
using nanoFramework.Runtime.Native;

namespace Proxy;

public class Program
{
    public static void Main()
    {
        try
        {
            Debug.WriteLine("Hello from nanoFramework!");
            var settings = ReadSettings();
            WifiHelper.ConnectToWifi(settings);

            using var shutdownTokenSource = new CancellationTokenSource();
            var builder = new HostBuilder();
            builder.Properties[nameof(Settings)] = settings;
            builder.Properties[nameof(CancellationTokenSource)] = shutdownTokenSource;
            builder.ConfigureServices(ConfigureServices);
            builder.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);

            using var host = builder.Build();

            var webServer = (MyWebServer)host.Services.GetRequiredService(typeof(MyWebServer));
            webServer.Start();
            Debug.WriteLine("HTTP server started");

            host.StartAsync(shutdownTokenSource.Token);
            shutdownTokenSource.Token.WaitHandle.WaitOne();
            Debug.WriteLine("Shutdown token was cancelled");

            webServer.Stop();
            host.StopAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Format());
        }

        Power.RebootDevice();
    }

    private static Settings ReadSettings()
    {
        var settingsJson  = File.ReadAllText(Settings.FilePath);
        Debug.WriteLine($"Read settings: {settingsJson}");
        return (Settings)JsonConvert.DeserializeObject(settingsJson, typeof(Settings));
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(typeof(Settings), context.Properties[nameof(Settings)]);
        services.AddSingleton(typeof(CancellationTokenSource), context.Properties[nameof(CancellationTokenSource)]);
        services.AddSingleton(typeof(Ping));
        services.AddSingleton(typeof(State));
        services.AddSingleton(typeof(WakeOnLan));
        services.AddSingleton(typeof(MyWebServer));
        services.AddSingleton(typeof(ApiConnection));
        services.AddHostedService(typeof(StateWorker));
    }
}