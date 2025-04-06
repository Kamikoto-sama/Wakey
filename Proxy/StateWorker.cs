using System;
using System.Diagnostics;
using System.Threading;
using Api.Contracts;
using Microsoft.Extensions.Hosting;

namespace Proxy;

public class StateWorker(
    State state,
    Ping ping,
    WakeOnLan wakeOnLan,
    ApiConnection apiConnection,
    CancellationTokenSource shutdownToken
) : BackgroundService
{
    protected override void ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            apiConnection.On(ProxyMethods.Awake, [typeof(bool)], HandleAwake);
            apiConnection.On(ProxyMethods.Reboot, [typeof(bool)], HandleReboot);
            apiConnection.Start();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Api connection failed: {e.Format()}");
        }

        Debug.WriteLine("Start checking status");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                state.PingSucceed = ping.Send(Constants.DpcIp);
                var dto = new ProxyStatusDto { PingSucceed = state.PingSucceed };
                apiConnection.Send(StatusHubMethods.SyncProxyStatus, dto);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to check state: {e.Message}");
            }

            Thread.Sleep(5000);
        }
    }

    private void HandleReboot(object sender, object[] args)
    {
        var reboot = (bool)args[0];
        if (reboot)
            shutdownToken.Cancel();
    }

    private void HandleAwake(object sender, object[] args)
    {
        var awake = (bool)args[0];
        if (awake)
            wakeOnLan.SendMagicPacket();
    }
}