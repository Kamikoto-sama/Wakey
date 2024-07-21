using Daemon;

var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine(Directory.GetCurrentDirectory());
builder.Logging.AddFile(
    @"D:\Projects\WakeOnLan\Wakey\Daemon\bin\Release\publish\logs\log_{Date}",
    retainedFileCountLimit: 7
);

builder.Services.AddWindowsService();
builder.Services.AddSingleton<VpnService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();