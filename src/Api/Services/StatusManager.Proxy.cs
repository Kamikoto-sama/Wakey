using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void Awake() => Send(ClientType.Proxy, ProxyMethods.Awake, true);
    
    public void RebootProxy() => Send(ClientType.Proxy, ProxyMethods.Reboot, true);

    public void UpdateProxyStatus(ProxyStatusDto dto)
    {
        status.ProxyConnected = true;
        status.ProxyLastUpdate = DateTime.UtcNow;
        status.PingSucceed = dto.PingSucceed;
    }

    public void ProxyConnected() => status.ProxyConnected = true;

    public void ProxyDisconnected()
    {
        status.ProxyConnected = false;
        status.PingSucceed = false;
    }
}
