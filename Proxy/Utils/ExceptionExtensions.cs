using System;
using System.Text;

namespace Proxy.Utils;

public static class ExceptionExtensions
{
    public static string Format(this Exception? exception)
    {
        var message = new StringBuilder();
        var level = 0;
        while (exception != null)
        {
            var prefix = new string('-', level);
            message.AppendLine(prefix + exception);
            exception = exception.InnerException;
            level++;
        }

        return message.ToString();
    }    
}