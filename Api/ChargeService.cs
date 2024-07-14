using Kami.Utils;

namespace Api;

public class ChargeService(IConfiguration configuration, HttpClient client, ILogger<ChargeService> log)
{
    private const int MaxLevel = 80;
    private const int MinLevel = 20;

    private readonly AsyncLock asyncLock = new();

    public async Task ChargeBattery(BatteryDto battery)
    {
        switch (battery)
        {
            case { Level: >= MaxLevel, Charging: true }:
                await Charge(false);
                break;
            case { Level: <= MinLevel, Charging: false }:
                await Charge(true);
                break;
        }
    }

    private async Task Charge(bool enable)
    {
        if (asyncLock.Locked)
        {
            log.LogInformation("Charge lock is taken");
            return;
        }

        using var @lock = await asyncLock.Obtain();
        try
        {
            await SwitchDevice(enable);
        }
        catch (Exception e)
        {
            log.LogError(e, "Failed to switch charge to {}", enable);
        }
    }

    private async Task SwitchDevice(bool enable)
    {
        var options = configuration.Get<YandexApiOptions>(YandexApiOptions.Key)!;
        var body = new
        {
            devices = new[]
            {
                new
                {
                    id = options.DeviceId,
                    actions = new[]
                    {
                        new
                        {
                            type = "devices.capabilities.on_off",
                            state = new
                            {
                                instance = "on",
                                value = enable
                            }
                        }
                    }
                }
            }
        };

        var content = JsonContent.Create(body);
        var res = await client.PostAsync("devices/actions", content);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"{res.StatusCode} - {res.ReasonPhrase}; '{await res.Content.ReadAsStringAsync()}'");
        log.LogInformation("Successfully switched device to {}", enable);
    }
}