using System;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nanoFramework.WebServer;
using Proxy.Utils;

namespace Proxy.Services;

public class MyWebServer(ILogger logger, IServiceProvider serviceProvider) : WebServer(80, HttpProtocol.Http, [typeof(IndexController)])
{
    protected override void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
    {
        var request = context.Request;
        var routeStr = $"{request.RemoteEndPoint} {request.HttpMethod} {request.RawUrl}";
        try
        {
            logger.LogDebug($"Request from ${routeStr}");
            var controllerInstance = ActivatorUtilities.CreateInstance(serviceProvider, route.Callback.DeclaringType);
            route.Callback.Invoke(controllerInstance, [new WebServerEventArgs(context)]);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Failed to invoke route: {routeStr}");
        }
    }
}