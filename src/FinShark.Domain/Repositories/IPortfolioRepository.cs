using FinShark.Domain.Entities;

namespace FinShark.Domain.Repositories;

/// <summary>
/// Repository interface for portfolio-specific operations.
/// Keeps portfolio use cases separated from stock CRUD concerns.
/// </summary>
public interface IPortfolioRepository
{
    Task<IEnumerable<Stock>> GetPortfolioAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AddStockToPortfolioAsync(string userId, int stockId, CancellationToken cancellationToken = default);
    Task<bool> RemoveStockFromPortfolioAsync(string userId, int stockId, bool hardDelete = false, CancellationToken cancellationToken = default);
}
