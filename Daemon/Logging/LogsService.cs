using System.Threading.Channels;
using Api.Contracts;
using Kami.Utils;

namespace Daemon.Logging;

public class LogsService : BackgroundService
{
    private const int RetryDelay = 1000;
    private readonly ApiConnection apiConnection;
    private readonly Channel<LogDto> logsQueue;

    public LogsService(ApiConnection apiConnection)
    {
        this.apiConnection = apiConnection;
        var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropOldest };
        logsQueue = Channel.CreateBounded<LogDto>(options);
    }

    public void SendLog(LogDto log) => logsQueue.Writer.TryWrite(log);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var log in logsQueue.Reader.ReadAllAsync(stoppingToken))
            await Send(log, stoppingToken);
    }

    private async Task Send(LogDto log, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (await apiConnection.TrySendAsync(StatusHubMethods.Log, cancellationToken, log))
                    return;
            }
            catch (Exception)
            {
                await Task.Delay(RetryDelay, cancellationToken).HandleCancellation();
            }
        }
    }
}