using System.Net.Http.Headers;

namespace Api;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddConsole(options => options.TimestampFormat = "[yyyy.MM.dd HH:mm:ss.fff] ");

        builder.Services.AddSingleton<AliceService>();
        builder.Services.AddSingleton<StatusManager>();
        builder.Services.AddSingleton<ChargeService>();
        builder.Services.AddHttpClient<ChargeService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IConfiguration>().Get<YandexApiOptions>(YandexApiOptions.Key)!;
            client.BaseAddress = new Uri(options.Url);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        });

        builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
        builder.WebHost.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
        var app = builder.Build();

        app.MapGet("/status", (StatusManager manager) => manager.GetStatus());
        app.MapPost("/wake", (StatusManager manager) => manager.Wake());
        app.MapPost("/awake/reset", (StatusManager manager) => manager.ResetAwake());
        app.MapPost("/vpn/enable", (StatusManager manager) => manager.EnableVpn());
        app.MapPost("/vpn/reset", (StatusManager manager) => manager.ResetVpn());

        app.MapPost("/proxy/status", (ProxyStatusDto dto, StatusManager manager) => manager.UpdateProxyStatus(dto));
        app.MapPost("/daemon/status", (DaemonStatusDto dto, StatusManager manager) => manager.UpdateDaemonStatus(dto));

        app.MapPost("/alice", (AliceCommandDto commandDto, AliceService aliceService) => aliceService.HandleCommand(commandDto));
        
        app.Run();
    }
}