using System;
using System.Diagnostics;
using System.Reflection;
using Api.Contracts;
using Microsoft.Extensions.Logging;
using Proxy.Services;
using Proxy.Utils;

namespace Proxy.Logging;

public class ApiLogger(ApiConnection apiConnection, LogLevel minimumLevel) : ILogger
{
    public void Log(LogLevel logLevel, EventId eventId, string state, Exception exception, MethodInfo format)
    {
        try
        {
            var message = state;
            var errorMessage = exception.Format();
            if (!string.IsNullOrEmpty(errorMessage))
                message += "\n" + exception.Format();

            var log = new LogDto { Level = (int)logLevel, Message = message };
            apiConnection.Send(StatusHubMethods.Log, log);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Format());
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;
}