using Daemon;
using Daemon.Logging;

var builder = Host.CreateApplicationBuilder(args);

var logsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log_{Date}");
builder.Logging.AddFile(logsFilePath, retainedFileCountLimit: 7);
var apiLoggerProvider = new ApiLoggerProvider();
builder.Logging.AddProvider(apiLoggerProvider);

var settings = builder.Configuration.Get<Settings>()!;

builder.Services.AddSingleton<ApiConnection>();
builder.Services.AddSingleton(settings);
builder.Services.AddWindowsService();
builder.Services.AddSingleton<RdpForwardingService>();
builder.Services.AddHostedService<Worker>();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
AppDomain.CurrentDomain.UnhandledException +=
    (_, args) => logger.LogError(args.ExceptionObject as Exception, "Unhandled exception");
TaskScheduler.UnobservedTaskException += 
    (_, args) => logger.LogError(args.Exception, "Unobserved task exception");

try
{
    var apiConnection = host.Services.GetRequiredService<ApiConnection>();
    apiLoggerProvider.ApiConnection = apiConnection;
    await apiConnection.StartAsync(CancellationToken.None);

    await host.RunAsync();
}
catch (Exception e)
{
    logger.LogError(e, "Failed to start Daemon");
}