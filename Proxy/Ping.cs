using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Proxy;

public class Ping : IDisposable
{
    private static readonly byte[] EchoRequestPacket = [8, 0, 247, 255, 0, 0, 0, 0];
    private readonly Socket socket = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp) { ReceiveTimeout = 3000 };

    public bool Send(string ipAddress)
    {
        try
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), IPEndPoint.MinPort);
            socket.SendTo(EchoRequestPacket, endPoint);

            var replyBuffer = new byte[256];
            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var received = socket.ReceiveFrom(replyBuffer, ref remoteEndpoint);
            return received > 0;
        }
        catch (SocketException e) when(e.ErrorCode == (int)SocketError.TimedOut)
        {
            Debug.WriteLine("Ping timed out");
            return false;
        }
    }

    public void Dispose() => ((IDisposable)socket).Dispose();
}