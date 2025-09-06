using Api.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Api.Filters;

public class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    private const string ApiKeyParam = "apiKey";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var apiKey = configuration.Get<AuthOptions>(AuthOptions.Key)!.ApiKey;
        var requestQuery = context.HttpContext.Request.Query;
        if (requestQuery.TryGetValue(ApiKeyParam, out var clientApiKey) && apiKey == clientApiKey)
            return await next(context);
        return new UnauthorizedObjectResult("Invalid API key");
    }
}