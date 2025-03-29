using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Proxy;

public class WakeOnLan
{
    private readonly byte[] magicPacket;
    private readonly UdpClient client;

    public WakeOnLan()
    {
        magicPacket = BuildMagicPacket();
        client = new UdpClient { EnableBroadcast = true };
        var endPoint = new IPEndPoint(IPAddress.Parse(Constants.DpcIp), 9);
        client.Connect(endPoint);
    }

    public void SendMagicPacket()
    {
        client.Send(magicPacket);
        Debug.WriteLine("Magic packet sent");
    }

    private static byte[] BuildMagicPacket()
    {
        var macBytes = new byte[6];
        for (var i = 0; i < 6; i++) macBytes[i] = Convert.ToByte(Constants.DpcMac.Substring(i * 2, 2), 16);

        var packet = new MemoryStream();
        for (var i = 0; i < 6; i++)
            packet.WriteByte(0xFF);
        for (var i = 0; i < 16; i++)
            packet.Write(macBytes, 0, macBytes.Length);
        return packet.ToArray();
    }
}