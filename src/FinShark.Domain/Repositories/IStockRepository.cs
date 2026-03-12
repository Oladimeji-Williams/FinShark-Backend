using FinShark.Domain.Entities;
using FinShark.Domain.Interfaces;

namespace FinShark.Domain.Repositories;

/// <summary>
/// Repository interface for Stock entity
/// Inherits standard CRUD operations from IRepository{Stock}
/// Defines stock-specific data access operations
/// </summary>
public interface IStockRepository : IRepository<Stock>
{
    /// <summary>
    /// Retrieves a stock by its symbol (ticker code)
    /// </summary>
    /// <param name="symbol">The stock symbol to search for (e.g., "AAPL", "MSFT")</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Stock entity if found; otherwise null</returns>
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a stock by its ID including associated comments
    /// </summary>
    /// <param name="id">Stock ID</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Stock entity with comments if found; otherwise null</returns>
    Task<Stock?> GetByIdWithCommentsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all stocks including associated comments
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Stocks with comments</returns>
    Task<IEnumerable<Stock>> GetAllWithCommentsAsync(CancellationToken cancellationToken = default);
}
