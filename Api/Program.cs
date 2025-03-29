using Api.Endpoints;
using Api.Filters;
using Api.Services;

namespace Api;

internal static class Program
{
    [Obsolete("Obsolete")]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddConsole(options => options.TimestampFormat = "[yyyy.MM.dd HH:mm:ss.fff] ");

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<AliceService>();
        builder.Services.AddSingleton<StatusManager>();

        builder.WebHost.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
        var app = builder.Build();

        app.MapControllers()
            .AddEndpointFilter<ControllerActionEndpointConventionBuilder, ApiKeyFilter>();
        app.MapHub<StatusHub>("/status")
            .AddEndpointFilter<HubEndpointConventionBuilder, ApiKeyFilter>()
            .AddEndpointFilter<HubEndpointConventionBuilder, StatusHubFilter>();

        app.Run();
    }
}