using System.ServiceProcess;
using Daemon;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();
builder.Services.AddSingleton<VpnService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();