namespace Api.Contracts;

public record DaemonStatusDto
{
    public bool VpnEnabled { get; init; }
}