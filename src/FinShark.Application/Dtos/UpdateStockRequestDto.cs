namespace FinShark.Application.Dtos;

using FinShark.Domain.Enums;

/// <summary>
/// DTO for updating an existing stock - supports partial updates with optional fields
/// </summary>
public sealed class UpdateStockRequestDto
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
    /// Industry classification (optional for partial updates)
    /// </summary>
    public IndustryType? Industry { get; init; }

    /// <summary>
    /// Market capitalization (optional for partial updates)
    /// </summary>
    public decimal? MarketCap { get; init; }
}
