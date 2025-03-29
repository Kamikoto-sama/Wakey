using System;
using nanoFramework.WebServer;

namespace Proxy;

public class IndexController(WakeOnLan wakeOnLan, State state)
{
    [Method("GET"), Route("/")]
    public void GetIndex(WebServerEventArgs eventArgs)
    {
        var context = eventArgs.Context;
        var content = GetContent();
        context.Response.ContentType = "text/html";
        WebServer.OutPutStream(context.Response, content);
    }

    [Method("GET"), Route("wake")]
    public void Wake(WebServerEventArgs eventArgs) => wakeOnLan.SendMagicPacket();

    private string GetContent()
    {
        var time = DateTime.UtcNow;
        //lang=html
        return $$"""
                 <html lang="en">
                 <div id="time"></div>
                 <div>Status: {{state.PingSucceed}}</div>
                 <button id="wake" onclick="wake()">Wake</button>
                 <script>
                 async function wake(){
                     const wakeBtn = document.getElementById('wake');
                     wakeBtn.disabled = true;
                     await fetch("/wake");
                     wakeBtn.disabled = false;
                 }
                 
                 const localTime = new Date('{{time}} UTC');
                 const timeLabel = document.getElementById('time');
                 timeLabel.innerText = localTime.toLocaleString('ru-ru').replace(',', '');
                 </script>
                 </html>
                 """;
    }
}