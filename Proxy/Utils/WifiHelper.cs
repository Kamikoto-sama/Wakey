using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using nanoFramework.Networking;

namespace Proxy.Utils;

public static class WifiHelper
{
    public static void ConnectToWifi(Settings.Settings settings)
    {
        var cts = new CancellationTokenSource(10 * 1000);
        var success = WifiNetworkHelper.ConnectDhcp(settings.WifiSsid, settings.WifiPassword, requiresDateTime: true, token: cts.Token);
        if (!success)
            throw new Exception($"Can't get a proper IP address and DateTime, error: {NetworkHelper.Status}", NetworkHelper.HelperException);

        using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var epGoogleDns = new IPEndPoint(new IPAddress([8, 8, 8, 8]), 53);
        sock.Connect(epGoogleDns);
        Debug.WriteLine($"{DateTime.UtcNow} WiFi connected on: {((IPEndPoint)sock.LocalEndPoint).Address}");
    }
}