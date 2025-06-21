using System.Diagnostics;
using Kami.Utils;

namespace Daemon;

public class SteamService : IDisposable
{
    private const string SteamLauncherPath =
        @"C:\Users\Dan\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Steam\Steam.lnk";

    private const string ProcessName = "steam";

    private readonly ILogger<SteamService> logger;
    private Process? steamProcess;
    private readonly CancellationTokenSource cts;
    private readonly Task checkerTask;

    public SteamService(ILogger<SteamService> logger)
    {
        this.logger = logger;
        cts = new CancellationTokenSource();
        checkerTask = CheckStatus();
    }

    public bool Running => steamProcess?.HasExited == false;

    public void Launch()
    {
        if (Running)
            return;

        var process = Process.Start("explorer.exe", SteamLauncherPath);
        if (process.HasExited)
            logger.LogError("Steam exited with code {ProcessExitCode}", process.ExitCode);
    }

    public void Kill()
    {
        if (!Running)
            return;
        steamProcess?.Kill();
    }

    private async Task CheckStatus()
    {
        while (!cts.IsCancellationRequested)
        {
            if (!Running)
                steamProcess = Process.GetProcessesByName(ProcessName).FirstOrDefault();
            await Task.Delay(1000, cts.Token).HandleCancellation();
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        checkerTask.Wait();
    }
}