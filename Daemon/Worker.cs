using Api.Contracts;

namespace Daemon;

public class Worker(ILogger<Worker> logger, VpnService vpnService, ApiConnection connection)
    : IHostedService
{
    private readonly Status status = new();
    private readonly CancellationTokenSource cts = new();
    private Task? statusReporter;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        connection.On<bool>("Vpn", HandleVpn);
        statusReporter = await Task.Factory.StartNew(StatusReporter, TaskCreationOptions.LongRunning);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        vpnService.Stop();
        await cts.CancelAsync();
        if (statusReporter != null)
            await statusReporter;
    }

    private async Task StatusReporter()
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                status.VpnEnabled = vpnService.Running;
                await connection.SendAsync(StatusHubMethods.SyncDaemonStatus, cts.Token, status);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send status");
            }

            try
            {
                await Task.Delay(5000, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private void HandleVpn(bool vpnRequested)
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

    private void StartVpn()
    {
        logger.LogInformation("Starting vpn...");
        vpnService.Start();
        status.VpnEnabled = vpnService.Running;
    }

    private void StopVpn()
    {
        logger.LogInformation("Killing vpn...");
        vpnService.Stop();
    }
}

public record Status
{
    public bool VpnEnabled { get; set; }
}