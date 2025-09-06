namespace Api.Contracts;

public static class UrlBuilder
{
    public static string BuildStatusUrl(string apiBaseUrl, string apiKey, ClientType clientType)
    {
        return $"{apiBaseUrl}/status?apiKey={apiKey}&clientType={clientType.ToString()}";
    }
}