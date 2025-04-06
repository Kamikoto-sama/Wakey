using System;
using System.Threading;
using nanoFramework.WebServer;

namespace Proxy;

public class IndexController(WakeOnLan wakeOnLan, State state, CancellationTokenSource shutdownTokenSource)
{
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
    public void Reboot(WebServerEventArgs eventArgs) => shutdownTokenSource.Cancel();

    private string GetContent()
    {
        var time = DateTime.UtcNow;
        //lang=html
        return $$"""
                 <html lang="en">
                 <div id="time"></div>
                 <div>Status: {{state.PingSucceed}}</div>
                 <button id="wake" onclick="wake()">Wake</button><br>
                 <button id="reboot" onclick="reboot()">Reboot</button>
                 <script>
                 async function wake(){
                     const wakeBtn = document.getElementById('wake');
                     wakeBtn.disabled = true;
                     await fetch("/wake", {method:"POST"});
                     wakeBtn.disabled = false;
                 }
                 async function reboot(){
                     const rebootBtn = document.getElementById('reboot');
                     rebootBtn.disabled = true;
                     await fetch("/reboot", {method:"POST"});
                     rebootBtn.disabled = false;
                 }

                 const localTime = new Date('{{time}} UTC');
                 const timeLabel = document.getElementById('time');
                 timeLabel.innerText = localTime.toLocaleString('ru-ru').replace(',', '');
                 </script>
                 </html>
                 """;
    }
}