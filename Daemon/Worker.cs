using Kami.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Daemon;

public class Worker : IHostedService, IAsyncDisposable
{
    private readonly ILogger<Worker> logger;
    private readonly VpnService vpnService;
    private readonly HubConnection connection;
    private readonly Status status = new();
    private readonly CancellationTokenSource cts;
    private Task? statusReporter;

    public Worker(ILogger<Worker> logger, VpnService vpnService, IConfiguration configuration)
    {
        this.logger = logger;
        this.vpnService = vpnService;
        var apiUri = configuration.GetValue<string>("ApiUri");
        var apiKey = configuration.GetValue<string>("ApiKey");
        connection = new HubConnectionBuilder()
            .WithUrl($"{apiUri}/status?apiKey={apiKey}&clientType=Daemon")
            .WithAutomaticReconnect(new ConstantDelayRetryPolicy(5.Seconds()))
            .Build();
        cts = new CancellationTokenSource();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await connection.StartAsync(cancellationToken);
        connection.On<bool>("Vpn", HandleResponse);
        statusReporter = await Task.Factory.StartNew(StatusReporter, TaskCreationOptions.LongRunning);
    }

    private void HandleResponse(bool vpnRequested)
    {
        logger.LogInformation($"Vpn requested: {vpnRequested}; Running: {vpnService.Running}");
        switch (vpnRequested)
        {
            case true when !vpnService.Running:
                StartVpn();
                break;
            case false when vpnService.Running:
                StopVpn();
                break;
        }
    }

    private void StopVpn()
    {
        logger.LogInformation("Killing vpn...");
        vpnService.Stop();
    }

    private void StartVpn()
    {
        logger.LogInformation("Starting vpn...");
        vpnService.Start();
        status.VpnEnabled = vpnService.Running;
    }

    private async Task StatusReporter()
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                status.VpnEnabled = vpnService.Running;
                await connection.SendAsync("SyncDaemonStatus", status, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Request failed");
            }

            try
            {
                await Task.Delay(3000, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        vpnService.Stop();
        await cts.CancelAsync();
        if (statusReporter != null)
            await statusReporter;
        await connection.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync() => await connection.DisposeAsync();
}

public record Status
{
    public bool VpnEnabled { get; set; }
}