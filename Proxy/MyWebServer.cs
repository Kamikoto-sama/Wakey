using System;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using nanoFramework.WebServer;

namespace Proxy;

public class MyWebServer(IServiceProvider serviceProvider) : WebServer(80, HttpProtocol.Http, [typeof(IndexController)])
{
    protected override void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            Debug.WriteLine($"Request from {request.RemoteEndPoint} {request.HttpMethod} {request.RawUrl}");
            var controllerInstance = ActivatorUtilities.CreateInstance(serviceProvider, route.Callback.DeclaringType);
            route.Callback.Invoke(controllerInstance, [new WebServerEventArgs(context)]);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
}