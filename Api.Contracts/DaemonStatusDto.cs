namespace Api.Contracts;

public record DaemonStatusDto
{
    public bool RdpForwardingEnabled { get; init; }
}