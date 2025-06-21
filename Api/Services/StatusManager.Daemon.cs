using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void EnableVpn() => Send(ClientType.Daemon, DaemonMethods.Vpn, true);

    public void DisableVpn() => Send(ClientType.Daemon, DaemonMethods.Vpn, false);
    
    public void RunSteam() => Send(ClientType.Daemon, DaemonMethods.Steam, true);

    public void KillSteam() => Send(ClientType.Daemon, DaemonMethods.Steam, false);

    public void UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonConnected = true;
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.VpnEnabled = dto.VpnEnabled;
        status.SteamRunning = dto.SteamRunning;
    }

    public void DaemonConnected() => status.DaemonConnected = true;
    
    public void DaemonDisconnected() => status.DaemonConnected = false;
}