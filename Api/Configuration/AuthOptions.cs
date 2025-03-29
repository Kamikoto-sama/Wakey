namespace Api.Configuration;

public record AuthOptions
{
    public const string Key = "Auth";

    public required string ApiKey { get; init; }
}