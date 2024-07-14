namespace Api.Configuration;

public record AuthOptions
{
    public const string Key = "Auth";

    public string ApiKey { get; init; }
}