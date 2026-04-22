namespace RailwayHelloApp.Models;

public sealed class DatabaseConnectionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ServerTimeUtc { get; init; }
}
