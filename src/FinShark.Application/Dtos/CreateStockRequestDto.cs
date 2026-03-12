namespace FinShark.Application.Dtos;

using FinShark.Domain.Enums;

/// <summary>
/// Request DTO for creating a new Stock
/// </summary>
public sealed class CreateStockRequestDto
{
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public IndustryType Industry { get; init; } = IndustryType.Other;
    public decimal MarketCap { get; init; }
}
