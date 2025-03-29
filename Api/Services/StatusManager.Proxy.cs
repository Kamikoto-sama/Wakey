using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void Awake() => Send(ClientType.Proxy, "Awake", true);

    public void UpdateProxyStatus(ProxyStatusDto dto)
    {
        status.ProxyLastUpdate = DateTime.UtcNow;
        status.PingSucceed = dto.PingSucceed;
    }

    public void ProxyConnected() => status.ProxyConnected = true;

    public void ProxyDisconnected() => status.ProxyConnected = false;
}
