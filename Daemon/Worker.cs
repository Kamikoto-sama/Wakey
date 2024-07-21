using System.Net.Http.Json;

namespace Daemon;

public class Worker(ILogger<Worker> logger, VpnService vpnService, IConfiguration configuration) : BackgroundService
{
    private readonly HttpClient client = new()
    {
        BaseAddress = new Uri(configuration.GetValue<string>("ApiUri")!)
    };

    private readonly Status status = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                status.VpnEnabled = vpnService.Running;
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        vpnService.Stop();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        vpnService.Dispose();
        client.Dispose();
        base.Dispose();
    }
}

public record Status
{
    public bool VpnEnabled { get; set; }
}