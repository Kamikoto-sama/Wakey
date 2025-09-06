namespace Api.Services;

public record Status
{
    public bool PingSucceed { get; set; }
    public bool ProxyConnected { get; set; }
    public DateTime ProxyLastUpdate { get; set; }

    public bool RdpForwardingRequested { get; set; }
    public bool RdpForwardingEnabled { get; set; }
    public bool DaemonConnected { get; set; }
    public DateTime DaemonLastUpdate { get; set; }
}