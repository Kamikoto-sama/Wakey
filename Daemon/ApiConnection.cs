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
    private readonly AsyncManualResetEvent reconnectionLock;
    private readonly CancellationTokenSource cts;
    private readonly string url;

    private Task? reconnectionTask;

    public ApiConnection(Settings settings, ILogger<ApiConnection> logger)
    {
        this.logger = logger;
        url = UrlBuilder.BuildStatusUrl(settings.ApiUrl, settings.ApiKey, ClientType.Daemon);
        hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect(new ConstantDelayRetryPolicy(5.Seconds()))
            .Build();
        hubConnection.Closed += OnClosed;

        cts = new CancellationTokenSource();
        reconnectionLock = new AsyncManualResetEvent(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (State != HubConnectionState.Disconnected)
            return;
        if (reconnectionTask != null)
            reconnectionTask = Task.Factory.StartNew(Reconnect, TaskCreationOptions.LongRunning).Unwrap();

        logger.LogInformation("Connecting to: {Url}", url);
        await hubConnection.StartAsync(cancellationToken);
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
        logger.LogError(exception, "Connection closed");
        reconnectionLock.Set();
        return Task.CompletedTask;
    }

    private async Task Reconnect()
    {
        while (!cts.IsCancellationRequested)
        {
            await reconnectionLock.WaitAsync();
            if (cts.IsCancellationRequested)
                break;

            try
            {
                if (State != HubConnectionState.Disconnected)
                {
                    reconnectionLock.Reset();
                    continue;
                }

                logger.LogWarning("Reconnecting...");
                await hubConnection.StopAsync();
                await StartAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Reconnect failed");
            }

            Thread.Sleep(5000);
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
        hubConnection.StopAsync();
    }
}