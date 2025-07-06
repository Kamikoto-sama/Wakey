using Microsoft.Extensions.Logging.Debug;

namespace Daemon.Logging;

public class ApiLoggerProvider : ILoggerProvider
{
    private readonly DebugLoggerProvider debugLoggerProvider = new();

    public LogsService? LogsService { get; set; }

    public ILogger CreateLogger(string categoryName)
    {
        return LogsService == null
            ? debugLoggerProvider.CreateLogger(categoryName)
            : new ApiLogger(LogsService);
    }

    public void Dispose()
    {
    }
}