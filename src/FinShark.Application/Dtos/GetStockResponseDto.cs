namespace FinShark.Application.Dtos;

using FinShark.Domain.Enums;

/// <summary>
/// Data Transfer Object for Stock entity (Read/Get operations)
/// </summary>
public sealed class GetStockResponseDto
{
    public required int Id { get; init; }
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public IndustryType Industry { get; init; } = IndustryType.Other;
    public decimal MarketCap { get; init; }
    public required DateTime Created { get; init; }
    public DateTime? Modified { get; init; }
}
