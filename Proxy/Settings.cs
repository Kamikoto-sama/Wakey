namespace Proxy;

public class Settings
{
    public const string FilePath = "I:\\settings.json";

    public string DpcMac { get; init; } = null!;
    public string DpcIp { get; init; } = null!;
    public string WifiSsid { get; init; } = null!;
    public string WifiPassword { get; init; } = null!;
    public string ApiUrl { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
}