namespace FinShark.Domain.Entities;

/// <summary>
/// Join entity linking ApplicationUser and Stock to represent a user portfolio entry
/// </summary>
public sealed class PortfolioItem : BaseEntity
{
    public required string UserId { get; init; }
    public ApplicationUser User { get; set; } = null!;

    public required int StockId { get; init; }
    public Stock Stock { get; set; } = null!;
}
