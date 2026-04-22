namespace RailwayHelloApp.Models;

public sealed class TestRecord
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
