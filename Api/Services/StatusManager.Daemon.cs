using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void EnableVpn()
    {
        status.VpnRequested = true;
        Send(ClientType.Daemon, "Vpn", true);
    }

    public void DisableVpn()
    {
        status.VpnRequested = false;
        Send(ClientType.Daemon, "Vpn", false);
    }

    public void UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonConnected = true;
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.VpnEnabled = dto.VpnEnabled;
    }

    public void DaemonDisconnected() => status.DaemonConnected = false;
}

public record DaemonStatusDto
{
    public bool VpnEnabled { get; init; }
}