using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void EnableVpn() => status.VpnRequested = true;

    public void DisableVpn() => status.VpnRequested = false;

    public void UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonConnected = true;
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.VpnEnabled = dto.VpnEnabled;

        if (status.VpnRequested != dto.VpnEnabled)
            Send(ClientType.Daemon, DaemonMethods.Vpn, status.VpnRequested);
    }

    public void DaemonConnected() => status.DaemonConnected = true;

    public void DaemonDisconnected()
    {
        status.DaemonConnected = false;
        status.VpnEnabled = false;
    }
}