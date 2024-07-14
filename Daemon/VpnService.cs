using System.ServiceProcess;

namespace Daemon;

public class VpnService : IDisposable
{
    private const string ServiceName = "WireGuardTunnel$DPC";
    private readonly ServiceController service = new(ServiceName);

    public bool Running
    {
        get
        {
            service.Refresh();
            return service.Status == ServiceControllerStatus.Running;
        }
    }

    public void Start()
    {
        try
        {
            if (Running || service.Status == ServiceControllerStatus.StartPending)
                return;
            service.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void Stop()
    {
        try
        {
            if (!Running)
                return;
            service.Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose() => service.Dispose();
}