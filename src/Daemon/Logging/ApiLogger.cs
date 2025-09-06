using Api.Contracts;

namespace Daemon.Logging;

public class ApiLogger(LogsService logsService) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        if (exception != null)
            message += Environment.NewLine + exception;
        var log = new LogDto { Level = (int)logLevel, Message = message };
        logsService.SendLog(log);
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}