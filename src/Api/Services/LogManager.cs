using System.Collections.Concurrent;
using Api.Contracts;

namespace Api.Services;

public class LogManager(ILoggerFactory loggerFactory)
{
    private const int LogHistoryCapacity = 100;
    private readonly ConcurrentQueue<LogItem> logs = new();

    public void LogMessage(ClientType clientType, LogDto log)
    {
        var logger = loggerFactory.CreateLogger(clientType.ToString());
        var level = (LogLevel)log.Level;
        logger.Log(level, "{}", log.Message);
        EnqueueLog(clientType, DateTime.UtcNow, level, log.Message);
    }

    public IEnumerable<LogItem> GetLogs(int count) => logs.OrderByDescending(x => x.Timestamp).Take(count);

    private void EnqueueLog(ClientType from, DateTime timestamp, LogLevel level, string message)
    {
        if (logs.Count >= LogHistoryCapacity)
            logs.TryDequeue(out _);

        var logItem = new LogItem(from, timestamp, level, message);
        logs.Enqueue(logItem);
    }
}

public record LogItem(ClientType from, DateTime Timestamp, LogLevel Level, string Message);