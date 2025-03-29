using Api.Contracts;
using Api.Filters;
using Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Api.Endpoints;

public class StatusHub(StatusManager statusManager, ILogger<StatusHub> logger) : Hub
{
    public override Task OnConnectedAsync()
    {
        var clientType = GetClientType();
        Groups.AddToGroupAsync(Context.ConnectionId, clientType.ToString());
        logger.LogInformation("{} connected", clientType);
        switch (clientType)
        {
            case ClientType.Proxy:
                statusManager.ProxyConnected();
                break;
            case ClientType.Daemon:
                statusManager.DaemonConnected();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return base.OnConnectedAsync();
    }

    public void SyncProxyStatus(ProxyStatusDto proxyStatus) => statusManager.UpdateProxyStatus(proxyStatus);

    public void SyncDaemonStatus(DaemonStatusDto daemonStatus) => statusManager.UpdateDaemonStatus(daemonStatus);

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var clientType = GetClientType();
        switch (clientType)
        {
            case ClientType.Proxy:
                statusManager.ProxyDisconnected();
                break;
            case ClientType.Daemon:
                statusManager.DaemonDisconnected();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        logger.LogInformation("{} disconnected", clientType);
        return base.OnDisconnectedAsync(exception);
    }

    private ClientType GetClientType() => (ClientType)Context.GetHttpContext()!.Items[StatusHubFilter.ClientTypeParam]!;
}