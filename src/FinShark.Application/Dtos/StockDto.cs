namespace FinShark.Application.Dtos;

using FinShark.Domain.ValueObjects;

/// <summary>
/// Response DTO for stock creation
/// Returns the newly created stock's ID
/// </summary>
public sealed class CreateStockResponseDto
{
    public required int Id { get; init; }
}



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
    public IReadOnlyList<GetCommentResponseDto> Comments { get; init; } = Array.Empty<GetCommentResponseDto>();
    public required DateTime Created { get; init; }
    public DateTime? Modified { get; init; }
}

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
