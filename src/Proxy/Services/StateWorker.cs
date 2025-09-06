using System;
using System.Diagnostics;
using System.Threading;
using Api.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using nanoFramework.Runtime.Native;
using Proxy.Utils;

namespace Proxy.Services;

public class StateWorker(
    State state,
    Ping ping,
    WakeOnLan wakeOnLan,
    ApiConnection apiConnection,
    ProxySettings settings,
    ILogger logger
) : BackgroundService
{
    protected override void ExecuteAsync(CancellationToken stoppingToken)
    {
        apiConnection.On(ProxyMethods.Awake, [typeof(bool)], HandleAwake);
        apiConnection.On(ProxyMethods.Reboot, [typeof(bool)], HandleReboot);

        logger.LogDebug("Start checking status");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                state.PingSucceed = ping.Send(settings.PcIp);
                var dto = new ProxyStatusDto { PingSucceed = state.PingSucceed };
                apiConnection.Send(StatusHubMethods.SyncProxyStatus, dto);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to check state");
            }

            Thread.Sleep(5000);
        }
    }

    private void HandleReboot(object sender, object[] args)
    {
        try
        {
            var reboot = (bool)args[0];
            if (reboot)
                Power.RebootDevice();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to handle reboot");
        }
    }

    private void HandleAwake(object sender, object[] args)
    {
        try
        {
            var awake = (bool)args[0];
            if (awake)
                wakeOnLan.SendMagicPacket();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to handle awake");
        }
    }
}