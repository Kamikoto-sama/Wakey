namespace Proxy.Settings;

public class Settings
{
    public const string FilePath = "I:\\settings.json";

    public string PcMac { get; init; } = null!;
    public string PcIp { get; init; } = null!;
    public string WifiSsid { get; init; } = null!;
    public string WifiPassword { get; init; } = null!;
    public string ApiUrl { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
}