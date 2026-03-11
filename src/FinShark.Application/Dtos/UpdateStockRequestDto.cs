namespace FinShark.Application.Dtos;

/// <summary>
/// DTO for updating an existing stock
/// </summary>
public sealed record UpdateStockRequestDto(
    string Symbol,
    string CompanyName,
    decimal CurrentPrice,
    string Industry,
    decimal MarketCap)
{
    /// <summary>
    /// Initializes a new instance of the UpdateStockRequestDto class
    /// </summary>
    public UpdateStockRequestDto() : this(string.Empty, string.Empty, 0, string.Empty, 0) { }
};
