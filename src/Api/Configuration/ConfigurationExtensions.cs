namespace Api.Configuration;

public static class ConfigurationExtensions
{
    public static T? Get<T>(this IConfiguration configuration, string key)
    {
        var section = configuration.GetRequiredSection(key);
        return section.Get<T>();
    }
}