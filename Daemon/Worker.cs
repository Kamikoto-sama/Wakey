using System.Net.Http.Json;

namespace Daemon;

public class Worker(ILogger<Worker> logger, VpnService vpnService) : BackgroundService
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("") };
    private readonly Status status = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var res = await client.PostAsync("/daemon/status", JsonContent.Create(status), stoppingToken);
                var responseString = await res.Content.ReadAsStringAsync(stoppingToken);
                if (res.IsSuccessStatusCode)
                    HandleResponse(responseString);
                else
                    logger.LogError($"Request failed: {res.StatusCode} - {res.ReasonPhrase}; '{responseString}'");
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Request failed");
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

    private void HandleResponse(string vpnRequestedStr)
    {
        var vpnRequested = bool.Parse(vpnRequestedStr);
        logger.LogInformation($"Vpn requested: {vpnRequested}; Running: {vpnService.Running}");
        if (vpnRequested)
            StartVpn();
        else
            StopVpn();
    }

    private void StopVpn()
    {
            status.VpnEnabled = vpnService.Running;
            vpnService.Stop();
            logger.LogInformation("Killing vpn...");
    }

    private void StartVpn()
    {
        vpnService.Start();
        status.VpnEnabled = vpnService.Running;
    }

    public override void Dispose()
    {
        client.Dispose();
        base.Dispose();
    }
}

public record Status
{
    public bool VpnEnabled { get; set; }
}