namespace Api.Configuration;

public record YandexApiOptions
{
    public const string Key = "YandexApi";
    
    public required string Url { get; init; }
    public required string Token { get; init; }
    public required Guid DeviceId { get; init; }
}