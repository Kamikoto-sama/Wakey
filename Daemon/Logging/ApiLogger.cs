using Api.Contracts;

namespace Daemon.Logging;

public class ApiLogger(ApiConnection apiConnection) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var log = new LogDto { Level = (int)logLevel, Message = message };
        _ = apiConnection.SendAsync(StatusHubMethods.Log, CancellationToken.None, log);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}