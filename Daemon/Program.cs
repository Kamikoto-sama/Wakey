using Daemon;

var builder = Host.CreateApplicationBuilder(args);

var logsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log_{Date}");
builder.Logging.AddFile(logsFilePath, retainedFileCountLimit: 7);

builder.Services.AddWindowsService();
builder.Services.AddSingleton<VpnService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();