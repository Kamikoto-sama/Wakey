namespace Api.Services;

public record Status
{
    public bool PingSucceed { get; set; }
    public bool ProxyConnected { get; set; }
    public DateTime ProxyLastUpdate { get; set; }

    public bool VpnRequested { get; set; }
    public bool VpnEnabled { get; set; }
    public bool SteamRunning { get; set; }
    public bool LoggedIn { get; set; }
    public bool DaemonConnected { get; set; }
    public DateTime DaemonLastUpdate { get; set; }
}