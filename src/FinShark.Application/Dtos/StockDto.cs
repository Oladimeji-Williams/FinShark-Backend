namespace FinShark.Application.Dtos;

/// <summary>
/// Data Transfer Object for Stock entity (Read operations)
/// </summary>
public sealed class StockDto
{
    public required int Id { get; init; }
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public string Industry { get; init; } = string.Empty;
    public decimal MarketCap { get; init; }
    public required DateTime Created { get; init; }
    public DateTime? Modified { get; init; }
}
