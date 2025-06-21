namespace Api.Contracts;

public class LogDto
{
    public int Level { get; init; }
    public string Message { get; init; } = null!;
}