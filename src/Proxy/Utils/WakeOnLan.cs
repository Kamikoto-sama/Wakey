using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Proxy.Utils;

public class WakeOnLan : IDisposable
{
    private readonly ILogger logger;
    private readonly byte[] magicPacket;
    private readonly UdpClient client;

    public WakeOnLan(Settings.Settings settings, ILogger logger)
    {
        this.logger = logger;
        magicPacket = BuildMagicPacket(settings.PcMac);
        client = new UdpClient { EnableBroadcast = true };
        var endPoint = new IPEndPoint(IPAddress.Parse(settings.PcIp), 9);
        client.Connect(endPoint);
    }

    public void SendMagicPacket()
    {
        client.Send(magicPacket);
        logger.LogInformation("Magic packet sent");
    }

    private static byte[] BuildMagicPacket(string macAddress)
    {
        var macBytes = new byte[6];

        for (var i = 0; i < 6; i++)
            macBytes[i] = Convert.ToByte(macAddress.Substring(i * 2, 2), 16);

        var packet = new MemoryStream();
        for (var i = 0; i < 6; i++)
            packet.WriteByte(0xFF);
        for (var i = 0; i < 16; i++)
            packet.Write(macBytes, 0, macBytes.Length);
        return packet.ToArray();
    }

    public void Dispose() => client.Dispose();
}