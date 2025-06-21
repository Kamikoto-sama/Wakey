using Api.Contracts;

namespace Daemon;

public class Worker(ILogger<Worker> logger, VpnService vpnService, SteamService steamService, ApiConnection connection)
    : IHostedService
{
    private readonly CancellationTokenSource cts = new();
    private Task? statusReporter;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        connection.On<bool>(DaemonMethods.Vpn, HandleVpn);
        connection.On<bool>(DaemonMethods.Steam, HandleSteam);
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
                var status = new DaemonStatusDto
                {
                    VpnEnabled = vpnService.Running,
                    SteamRunning = steamService.Running,
                };
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

    private void HandleSteam(bool enable)
    {
        logger.LogInformation($"Steam requested: {enable}; Running: {steamService.Running}");
        try
        {
            if (enable)
                steamService.Launch();
            else
                steamService.Kill();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute command on steam");
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
    }

    private void StopVpn()
    {
        logger.LogInformation("Killing vpn...");
        vpnService.Stop();
    }
}
