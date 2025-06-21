using System;
using System.IO;
using nanoFramework.Runtime.Native;
using nanoFramework.WebServer;
using Proxy.Utils;

namespace Proxy.Services;

public class IndexController(WakeOnLan wakeOnLan, State state)
{
    private const string IndexFilePath = "I:\\index.html";
    
    [Method("GET"), Route("/")]
    public void GetIndex(WebServerEventArgs eventArgs)
    {
        var context = eventArgs.Context;
        var content = GetContent();
        context.Response.ContentType = "text/html";
        WebServer.OutPutStream(context.Response, content);
    }

    [Method("POST"), Route("wake")]
    public void Wake(WebServerEventArgs eventArgs) => wakeOnLan.SendMagicPacket();

    [Method("POST"), Route("reboot")]
    public void Reboot(WebServerEventArgs eventArgs) => Power.RebootDevice();

    private string GetContent()
    {
        var index = File.ReadAllText(IndexFilePath)
            .Replace("%pingStatus%", state.PingSucceed.ToString())
            .Replace("%timestamp%", DateTime.UtcNow.ToString());
        return index;
    }
}