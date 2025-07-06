using Kami.Utils;
using Renci.SshNet;

namespace Daemon;

public class RdpForwardingService : IDisposable
{
    private const int DefaultRdpPort = 3389;
    
    private readonly ILogger<RdpForwardingService> logger;
    private readonly SshClient sshClient;
    private readonly ForwardedPortRemote forwardedPort;

    public RdpForwardingService(ILogger<RdpForwardingService> logger, IConfiguration config)
    {
        this.logger = logger;
        var options = config.GetRequiredSection(SshTunnelOptions.Key).Get<SshTunnelOptions>()!;

        var pkFile = new PrivateKeyFile(options.KeyFilePath);
        sshClient = new SshClient(options.Host, options.User, pkFile);
        sshClient.KeepAliveInterval = 30.Seconds();
        sshClient.ErrorOccurred += (_, args) => logger.LogError(args.Exception, "SSH error"); 
        forwardedPort = new ForwardedPortRemote(options.Port, "localhost", DefaultRdpPort);
        forwardedPort.Exception += (_, args) => logger.LogError(args.Exception, "Port forwarding error");
    }

    public bool Running => forwardedPort.IsStarted;

    public void Start()
    {
        if (Running)
            return;
        if (!sshClient.IsConnected)
            sshClient.Connect();
        if (!sshClient.ForwardedPorts.Any())
            sshClient.AddForwardedPort(forwardedPort);

        forwardedPort.Start();
        logger.LogInformation("RDP port forwarding started");
    }

    public void Stop()
    {
        if (!Running)
            return;

        forwardedPort.Stop();
        sshClient.Disconnect();
        logger.LogInformation("RDP port forwarding stopped");
    }

    public void Dispose()
    {
        Stop();
        forwardedPort.Dispose();
        sshClient.Dispose();
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