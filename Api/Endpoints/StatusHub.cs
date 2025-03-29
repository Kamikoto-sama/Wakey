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
        var clientTypeName = clientType switch
        {
            ClientType.Proxy => nameof(ClientType.Proxy),
            ClientType.Daemon => nameof(ClientType.Daemon),
            _ => throw new ArgumentOutOfRangeException()
        };
        Groups.AddToGroupAsync(Context.ConnectionId, clientTypeName);
        logger.LogInformation("{} connected", clientType);
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