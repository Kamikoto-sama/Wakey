using Api.Contracts;
using Api.Endpoints;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public partial class StatusManager(IHubContext<StatusHub> statusHub, ILogger<StatusManager> log)
{
    private readonly Status status = new();

    public Status GetStatus() => status;

    private void Send(ClientType clientType, string method, bool value)
    {
        statusHub.Clients.Group(clientType.ToString()).SendAsync(method, value);
        log.LogInformation("Called {}({}) of {}", method, value, clientType);
    }
}

public record Status
{
    public bool PingSucceed { get; set; }
    public bool ProxyConnected { get; set; }
    public DateTime ProxyLastUpdate { get; set; }

    public bool VpnEnabled { get; set; }
    public bool SteamRunning { get; set; }
    public bool DaemonConnected { get; set; }
    public DateTime DaemonLastUpdate { get; set; }
}