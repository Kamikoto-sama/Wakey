using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Proxy.Logging;

public class CompositeLogger(params ILogger[] loggers) : ILogger
{
    public void Log(LogLevel logLevel, EventId eventId, string state, Exception exception, MethodInfo format)
    {
        foreach (var logger in loggers)
            if (logger.IsEnabled(logLevel))
                logger.Log(logLevel, eventId, state, exception, format);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        foreach (var logger in loggers)
            if (logger.IsEnabled(logLevel))
                return true;
        return false;
    }
}