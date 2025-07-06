using Kami.Utils;
using Renci.SshNet;

namespace Daemon;

public class RdpForwardingService : IDisposable
{
    private const int DefaultRdpPort = 3389;

    private readonly Lock @lock = new();
    private readonly ILogger<RdpForwardingService> logger;
    private readonly SshClient sshClient;
    private readonly uint remotePort;

    public RdpForwardingService(ILogger<RdpForwardingService> logger, IConfiguration config)
    {
        this.logger = logger;
        var options = config.GetRequiredSection(SshTunnelOptions.Key).Get<SshTunnelOptions>()!;

        var pkFile = new PrivateKeyFile(options.KeyFilePath);
        sshClient = new SshClient(options.Host, options.User, pkFile);
        sshClient.KeepAliveInterval = 30.Seconds();
        sshClient.ErrorOccurred += (_, args) => logger.LogError(args.Exception, "SSH error");
        remotePort = options.Port;
    }

    public bool Running => sshClient.IsConnected && sshClient.ForwardedPorts.All(p => p.IsStarted);

    public void Start()
    {
        lock (@lock)
        {
            if (Running)
                return;
            if (!sshClient.IsConnected)
                sshClient.Connect();

            StopPortForwarding();
            var forwardedPort = new ForwardedPortRemote(remotePort, "localhost", DefaultRdpPort);
            forwardedPort.Exception += (_, args) => logger.LogError(args.Exception, "Port forwarding error");
            sshClient.AddForwardedPort(forwardedPort);
            forwardedPort.Start();

            logger.LogInformation("RDP port forwarding started");
        }
    }

    public void Stop()
    {
        if (!Running)
            return;

        StopPortForwarding();
        sshClient.Disconnect();
        logger.LogInformation("RDP port forwarding stopped");
    }

    public void Dispose()
    {
        StopPortForwarding();
        sshClient.Dispose();
    }

    private void StopPortForwarding()
    {
        var ports = sshClient.ForwardedPorts.ToArray();
        foreach (var forwardedPort in ports)
        {
            try
            {
                sshClient.RemoveForwardedPort(forwardedPort);
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }
            try
            {
                forwardedPort.Dispose();
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
            }
        }
    }
}

public class SshTunnelOptions
{
    public const string Key = "SshTunnel";

    public required string Host { get; init; }
    public required string User { get; init; }
    public required string KeyFilePath { get; init; }
    public required uint Port { get; init; }
}