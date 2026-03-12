using FinShark.Domain.Entities;

namespace FinShark.Domain.Repositories;

/// <summary>
/// Repository interface for Stock entity
/// Defines contract for stock data access operations
/// </summary>
public interface IStockRepository
{
    /// <summary>
    /// Retrieves a stock by its ID
    /// </summary>
    Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all stocks
    /// </summary>
    Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new stock to the database
    /// </summary>
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stock from the database
    /// </summary>
    Task DeleteAsync(Stock stock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a stock by its symbol
    /// </summary>
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing stock in the database
    /// </summary>
    Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default);
}