using FinShark.Domain.Entities;
using FinShark.Domain.Interfaces;
using FinShark.Domain.Queries;

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

    /// <summary>
    /// Retrieves stocks with optional filtering, sorting, and pagination
    /// </summary>
    /// <param name="queryParameters">Query parameters for filtering, sorting, and pagination</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Filtered stocks with comments</returns>
    Task<IEnumerable<Stock>> GetAllWithCommentsAsync(StockQueryParameters queryParameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves count of stocks with optional filtering
    /// </summary>
    /// <param name="queryParameters">Query parameters for filtering</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Total count of filtered stocks</returns>
    Task<int> GetCountAsync(StockQueryParameters queryParameters, CancellationToken cancellationToken = default);
}
