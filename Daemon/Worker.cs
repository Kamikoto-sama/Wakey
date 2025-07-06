using Api.Contracts;
using Kami.Utils;

namespace Daemon;

public class Worker(ILogger<Worker> logger, RdpForwardingService rdpForwardingService, ApiConnection connection)
    : IHostedService
{
    private const int StatusUpdateInterval = 5000;

    private readonly CancellationTokenSource cts = new();
    private Task? statusReporter;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        connection.On<bool>(DaemonMethods.RdpForwarding, HandleRdpForwarding);
        statusReporter = await Task.Factory.StartNew(StatusReporter, TaskCreationOptions.LongRunning);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        rdpForwardingService.Stop();
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
                var status = new DaemonStatusDto { RdpForwardingEnabled = rdpForwardingService.Running };
                await connection.SendAsync(StatusHubMethods.SyncDaemonStatus, cts.Token, status);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send status");
            }

            await Task.Delay(StatusUpdateInterval, cts.Token).HandleCancellation();
        }
    }

    private void HandleRdpForwarding(bool enable)
    {
        try
        {
            var running = rdpForwardingService.Running;
            logger.LogInformation("RDP forwarding requested: {Enable}; Running: {Running}", enable, running);
            if (enable)
                rdpForwardingService.Start();
            else
                rdpForwardingService.Stop();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute command on RDP forwarding");
        }
    }
}