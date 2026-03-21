using FinShark.Domain.ValueObjects;

namespace FinShark.Domain.Queries;

/// <summary>
/// Query parameters for filtering, sorting, and paginating stock lists.
/// Kept in domain layer for repository contracts.
/// </summary>
public sealed class StockQueryParameters
{
    public string? Symbol { get; init; }
    public string? CompanyName { get; init; }
    public SectorType? Sector { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinMarketCap { get; init; }
    public decimal? MaxMarketCap { get; init; }
    public StockSortBy SortBy { get; init; } = StockSortBy.Symbol;
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}

public enum StockSortBy
{
    Symbol,
    CompanyName,
    CurrentPrice,
    MarketCap,
    Created
}

public enum SortDirection
{
    Asc,
    Desc
}
