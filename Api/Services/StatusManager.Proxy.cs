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
        status.Battery = dto.Battery;
        if (dto.PingSucceed && status.AwakeRequested)
            ResetAwake();

        if (dto.Battery != null)
            _ = chargeService.ChargeBattery(dto.Battery);
    }

    public void ProxyDisconnected() => status.ProxyConnected = false;
}

public class ProxyStatusDto
{
    public bool PingSucceed { get; set; }
    public BatteryDto? Battery { get; set; }
}

public record BatteryDto
{
    public float Level { get; init; }
    public bool Charging { get; init; }
}