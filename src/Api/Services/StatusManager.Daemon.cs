using Api.Contracts;

namespace Api.Services;

public partial class StatusManager
{
    public void EnableRdpForwarding() => status.RdpForwardingRequested = true;

    public void DisableRdpForwarding() => status.RdpForwardingRequested = false;

    public void UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonConnected = true;
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.RdpForwardingEnabled = dto.RdpForwardingEnabled;

        if (status.RdpForwardingRequested != dto.RdpForwardingEnabled)
            Send(ClientType.Daemon, DaemonMethods.RdpForwarding, status.RdpForwardingRequested);
    }

    public void DaemonConnected() => status.DaemonConnected = true;

    public void DaemonDisconnected()
    {
        status.DaemonConnected = false;
        status.RdpForwardingEnabled = false;
    }
}