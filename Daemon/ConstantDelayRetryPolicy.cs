using Microsoft.AspNetCore.SignalR.Client;

namespace Daemon;

public class ConstantDelayRetryPolicy(TimeSpan delay) : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext) => delay;
}