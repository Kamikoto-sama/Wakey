using System.Diagnostics;
using System.ServiceProcess;

#pragma warning disable CA1416
namespace Daemon;

public class VpnService : IDisposable
{
    private const string TunnelName = "RU";
    private const string ServiceName = "WireGuardTunnel$" + TunnelName;
    private const string WireGuardExePath = @"C:\Program Files\WireGuard\wireguard.exe";
    private const string VpnConfigPath = $@"C:\Program Files\WireGuard\{TunnelName}.conf";

    private readonly ILogger<VpnService> logger;
    private readonly ReaderWriterLockSlim serviceLock = new();

    private ServiceController? service;

    public bool Running
    {
        get
        {
            service?.Refresh();
            return service?.Status == ServiceControllerStatus.Running;
        }
    }

    public VpnService(ILogger<VpnService> logger)
    {
        this.logger = logger;
        if (ServiceRegistered())
            service = new ServiceController(ServiceName);
    }

    public void Start()
    {
        serviceLock.EnterWriteLock();
        try
        {
            if (Running || service?.Status == ServiceControllerStatus.StartPending)
                return;
            service ??= CreateService();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Filed to start service");
        }
        finally
        {
            serviceLock.ExitWriteLock();
        }
    }

    public void Stop()
    {
        serviceLock.EnterWriteLock();
        try
        {
            if (!Running)
                return;
            service?.Dispose();
            service = null;
            UnregisterService();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to stop service");
        }
        finally
        {
            serviceLock.ExitWriteLock();
        }
    }

    private ServiceController CreateService()
    {
        if (!ServiceRegistered())
            EvaluateWireGuardCommand(["/installtunnelservice", VpnConfigPath]);
        logger.LogInformation("Service registered");
        service ??= new ServiceController(ServiceName);
        return service;
    }

    private void UnregisterService()
    {
        if (ServiceRegistered())
            EvaluateWireGuardCommand(["/uninstalltunnelservice", TunnelName]);
        logger.LogInformation("Service unregistered");
    }

    private static void EvaluateWireGuardCommand(IEnumerable<string> args)
    {
        var processStartInfo = new ProcessStartInfo(WireGuardExePath, args);
        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();
        process.WaitForExit();
    }

    private static bool ServiceRegistered() => ServiceController.GetServices().Any(s => s.ServiceName == ServiceName);

    public void Dispose()
    {
        service?.Dispose();
        UnregisterService();
    }
}