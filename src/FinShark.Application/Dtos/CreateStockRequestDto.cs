namespace FinShark.Application.Dtos;

/// <summary>
/// Request DTO for creating a new Stock
/// </summary>
public sealed class CreateStockRequestDto
{
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public string Industry { get; init; } = string.Empty;
    public decimal MarketCap { get; init; }
}
