using Api.Services;

namespace Api.Filters;

public class StatusHubFilter : IEndpointFilter
{
    public const string ClientTypeParam = "clientType";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!request.Query.TryGetValue(ClientTypeParam, out var clientTypeName) ||
            !Enum.TryParse(clientTypeName, out ClientType clientType))
            return Results.BadRequest("Invalid client type");

        context.HttpContext.Items[ClientTypeParam] = clientType;

        return await next(context);
    }
}