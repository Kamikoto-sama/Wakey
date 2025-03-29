using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void Awake()
    {
        status.AwakeRequested = true;
        Send(ClientType.Proxy, "Awake", true);
    }

    public void ResetAwake()
    {
        status.AwakeRequested = false;
        Send(ClientType.Proxy, "Awake", false);
    }

    public void UpdateProxyStatus(ProxyStatusDto dto)
    {
        status.ProxyConnected = true;
        status.ProxyLastUpdate = DateTime.UtcNow;
        status.PingSucceed = dto.PingSucceed;
        if (dto.PingSucceed && status.AwakeRequested)
            ResetAwake();
    }

    public void ProxyDisconnected() => status.ProxyConnected = false;
}
