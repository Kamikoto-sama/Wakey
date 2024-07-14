using Api.Endpoints;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public partial class StatusManager(ChargeService chargeService, IHubContext<StatusHub> statusHub)
{
    private readonly Status status = new();

    public Status GetStatus() => status;

    private void Send(ClientType clientType, string method, bool value) =>
        statusHub.Clients.Group(clientType.ToString()).SendAsync(method, value);
}

public record Status
{
    public bool AwakeRequested { get; set; }
    public bool PingSucceed { get; set; }
    public BatteryDto? Battery { get; set; } = new();
    public bool ProxyConnected { get; set; }
    public DateTime ProxyLastUpdate { get; set; }

    public bool VpnRequested { get; set; }
    public bool VpnEnabled { get; set; }
    public bool DaemonConnected { get; set; }
    public DateTime DaemonLastUpdate { get; set; }
}

public enum ClientType
{
    Proxy,
    Daemon
}