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
builder.Services.AddSingleton<VpnService>();
builder.Services.AddSingleton<SteamService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

var apiConnection = host.Services.GetRequiredService<ApiConnection>();
apiLoggerProvider.ApiConnection = apiConnection;
await apiConnection.StartAsync(CancellationToken.None);

await host.RunAsync();