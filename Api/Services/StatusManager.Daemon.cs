using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void EnableVpn() => Send(ClientType.Daemon, "Vpn", true);

    public void DisableVpn() => Send(ClientType.Daemon, "Vpn", false);

    public void UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonConnected = true;
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.VpnEnabled = dto.VpnEnabled;
    }

    public void DaemonConnected() => status.DaemonConnected = true;
    
    public void DaemonDisconnected() => status.DaemonConnected = false;
}

public record DaemonStatusDto
{
    public bool VpnEnabled { get; init; }
}