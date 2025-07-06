using Api.Contracts;
using Kami.Utils;
using Kami.Utils.SynchronizationTools;
using Microsoft.AspNetCore.SignalR.Client;

namespace Daemon;

public sealed class ApiConnection : IDisposable
{
    private readonly ILogger<ApiConnection> logger;
    private readonly HubConnection hubConnection;
    private HubConnectionState State => hubConnection.State;
    private readonly string url;

    public ApiConnection(Settings settings, ILogger<ApiConnection> logger)
    {
        this.logger = logger;
        url = UrlBuilder.BuildStatusUrl(settings.ApiUrl, settings.ApiKey, ClientType.Daemon);
        hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect(new ConstantDelayRetryPolicy(5.Seconds()))
            .Build();
        hubConnection.Closed += OnClosed;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (State != HubConnectionState.Disconnected)
            return;

        logger.LogInformation("Connecting to: {Url}", url);
        while (State == HubConnectionState.Disconnected)
        {
            try
            {
                await hubConnection.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to {Url}", url);
                await Task.Delay(1000, cancellationToken);
            }
        }

        logger.LogInformation("Connected state: {HubConnectionState}", State);
    }

    public async Task SendAsync(string methodName, CancellationToken token, params object[] args)
    {
        if (State != HubConnectionState.Connected)
            return;

        await hubConnection.SendCoreAsync(methodName, args, token);
    }

    public IDisposable On<T>(string methodName, Action<T> handler) => hubConnection.On(methodName, handler);

    private Task OnClosed(Exception? exception)
    {
        if (exception != null)
            logger.LogError(exception, "Connection closed");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        hubConnection.StopAsync();
    }
}