using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using nanoFramework.Json;
using nanoFramework.Logging.Debug;
using nanoFramework.Runtime.Native;
using Proxy.Logging;
using Proxy.Services;
using Proxy.Utils;

namespace Proxy;

public class Program
{
    public static void Main()
    {
        try
        {
            Debug.WriteLine("Hello from nanoFramework!");
            Run();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Format());
        }

        Power.RebootDevice();
    }

    private static void Run()
    {
        var settings = ReadSettings();
        WifiHelper.ConnectToWifi(settings);

        using var apiConnection = new ApiConnection(settings);
        apiConnection.Start();
        var logger = BuildLogger(apiConnection);

        using var shutdownTokenSource = new CancellationTokenSource();
        var builder = new HostBuilder();
        builder.Properties[nameof(ProxySettings)] = settings;
        builder.Properties[nameof(CancellationTokenSource)] = shutdownTokenSource;
        builder.Properties[nameof(ApiConnection)] = apiConnection;
        builder.Properties[nameof(ILogger)] = logger;
        builder.ConfigureServices(ConfigureServices);
        builder.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);

        using var host = builder.Build();

        var webServer = (MyWebServer)host.Services.GetRequiredService(typeof(MyWebServer));
        webServer.Start();
        logger.LogDebug($"HTTP server started on port {webServer.Port}: {webServer.IsRunning}");

        host.StartAsync(shutdownTokenSource.Token);
        shutdownTokenSource.Token.WaitHandle.WaitOne();
        logger.LogInformation("Shutdown token was cancelled");

        webServer.Stop();
        host.StopAsync(CancellationToken.None);
    }

    private static ProxySettings ReadSettings()
    {
        var settingsJson = File.ReadAllText(StaticFilePaths.SettingsFilePath);
        Debug.WriteLine($"Read settings: {settingsJson}");
        return (ProxySettings)JsonConvert.DeserializeObject(settingsJson, typeof(ProxySettings));
    }

    private static CompositeLogger BuildLogger(ApiConnection apiConnection)
    {
        var apiLogger = new ApiLogger(apiConnection, LogLevel.Information);
        var debugLogger = new DebugLogger("Proxy");
        return new CompositeLogger(debugLogger, apiLogger);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton(typeof(ProxySettings), context.Properties[nameof(ProxySettings)]);
        services.AddSingleton(typeof(CancellationTokenSource), context.Properties[nameof(CancellationTokenSource)]);
        services.AddSingleton(typeof(ApiConnection), context.Properties[nameof(ApiConnection)]);
        services.AddSingleton(typeof(ILogger), context.Properties[nameof(ILogger)]);
        services.AddSingleton(typeof(Ping));
        services.AddSingleton(typeof(State));
        services.AddSingleton(typeof(WakeOnLan));
        services.AddSingleton(typeof(MyWebServer));
        services.AddHostedService(typeof(StateWorker));
    }
}