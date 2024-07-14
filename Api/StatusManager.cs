namespace Api;

public class StatusManager
{
    private readonly ChargeService chargeService;
    private readonly Status status = new();

    public Status GetStatus() => status;

    public StatusManager(ChargeService chargeService)
    {
        this.chargeService = chargeService;
    }

    public void Wake() => status.AwakeRequested = true;

    public void ResetAwake() => status.AwakeRequested = false;

    public void EnableVpn() => status.VpnRequested = true;

    public void ResetVpn() => status.VpnRequested = false;

    public bool UpdateProxyStatus(ProxyStatusDto dto)
    {
        var awakeRequested = status.AwakeRequested;

        status.ProxyLastUpdate = DateTime.UtcNow;
        status.PingSucceed = dto.PingSucceed;
        status.Battery = dto.Battery;
        if (dto.PingSucceed)
            status.AwakeRequested = false;

        if (dto.Battery != null)
            _ = chargeService.ChargeBattery(dto.Battery);

        return awakeRequested;
    }

    public bool UpdateDaemonStatus(DaemonStatusDto dto)
    {
        status.DaemonLastUpdate = DateTime.UtcNow;
        status.VpnEnabled = dto.VpnEnabled;
        return status.VpnRequested;
    }
}

public record DaemonStatusDto
{
    public bool VpnEnabled { get; init; }
}

public record ProxyStatusDto
{
    public bool PingSucceed { get; init; }
    public BatteryDto? Battery { get; init; }
}

public record BatteryDto
{
    public float Level { get; init; }
    public bool Charging { get; init; }
}

public record Status
{
    public bool AwakeRequested { get; set; }
    public bool PingSucceed { get; set; }
    public BatteryDto? Battery { get; set; }
    public DateTime? ProxyLastUpdate { get; set; }

    public DateTime? DaemonLastUpdate { get; set; }
    public bool VpnRequested { get; set; }
    public bool VpnEnabled { get; set; }
}