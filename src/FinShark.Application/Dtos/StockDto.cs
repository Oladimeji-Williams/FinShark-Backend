namespace FinShark.Application.Dtos;

using FinShark.Domain.ValueObjects;

/// <summary>
/// Response DTO for stock creation
/// Returns the newly created stock's ID
/// </summary>
public sealed record CreateStockResponseDto
{
    public required int Id { get; init; }
}



/// <summary>
/// Data Transfer Object for Stock entity (Read/Get operations)
/// </summary>
public sealed record GetStockResponseDto
{
    public required int Id { get; init; }
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public SectorType Sector { get; init; } = SectorType.Other;
    public decimal MarketCap { get; init; }
    public IReadOnlyList<GetCommentResponseDto> Comments { get; init; } = Array.Empty<GetCommentResponseDto>();
}

/// <summary>
/// Request DTO for creating a new Stock
/// </summary>
public sealed record CreateStockRequestDto
{
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required decimal CurrentPrice { get; init; }
    public SectorType Sector { get; init; } = SectorType.Other;
    public decimal MarketCap { get; init; }
}

/// <summary>
/// DTO for updating an existing stock - supports partial updates with optional fields
/// </summary>
public sealed record UpdateStockRequestDto
{
    /// <summary>
    /// Stock symbol (optional for partial updates)
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Company name (optional for partial updates)
    /// </summary>
    public string? CompanyName { get; init; }

    /// <summary>
    /// Current stock price (optional for partial updates)
    /// </summary>
    public decimal? CurrentPrice { get; init; }

    /// <summary>
    /// Sector classification (optional for partial updates)
    /// </summary>
    public SectorType? Sector { get; init; }

    /// <summary>
    /// Market capitalization (optional for partial updates)
    /// </summary>
    public decimal? MarketCap { get; init; }
}

/// <summary>
/// Response DTO for full FMP quote profile results.
/// </summary>
public sealed record GetFullStockQuoteResponseDto
{
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public required decimal Beta { get; init; }
    public required long VolAvg { get; init; }
    public required long MktCap { get; init; }
    public required decimal LastDiv { get; init; }
    public required string Range { get; init; }
    public required decimal Changes { get; init; }
    public required string CompanyName { get; init; }
    public required string Currency { get; init; }
    public required string Cik { get; init; }
    public required string Isin { get; init; }
    public required string Cusip { get; init; }
    public required string Exchange { get; init; }
    public required string ExchangeShortName { get; init; }
    public required string Sector { get; init; }
    public required string Website { get; init; }
    public required string Description { get; init; }
    public required string Ceo { get; init; }
    public required string Country { get; init; }
    public required string FullTimeEmployees { get; init; }
    public required string Phone { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Zip { get; init; }
    public required decimal DcfDiff { get; init; }
    public required decimal Dcf { get; init; }
    public required string Image { get; init; }
    public required string IpoDate { get; init; }
    public required bool DefaultImage { get; init; }
    public required bool IsEtf { get; init; }
    public required bool IsActivelyTrading { get; init; }
    public required bool IsAdr { get; init; }
    public required bool IsFund { get; init; }
}
