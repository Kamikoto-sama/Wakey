using Microsoft.Extensions.Logging.Debug;

namespace Daemon.Logging;

public class ApiLoggerProvider : ILoggerProvider
{
    private readonly DebugLoggerProvider debugLoggerProvider = new();

    public ApiConnection? ApiConnection { get; set; }

    public ILogger CreateLogger(string categoryName)
    {
        return ApiConnection == null
            ? debugLoggerProvider.CreateLogger(categoryName)
            : new ApiLogger(ApiConnection);
    }

    public void Dispose()
    {
    }
}