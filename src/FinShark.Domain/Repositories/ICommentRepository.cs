using FinShark.Domain.Entities;
using FinShark.Domain.Interfaces;

namespace FinShark.Domain.Repositories;

/// <summary>
/// Repository interface for Comment entity
/// Inherits standard CRUD operations from IRepository{Comment}
/// Defines comment-specific data access operations with pagination support
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// Retrieves all comments for a specific stock
    /// </summary>
    /// <param name="stockId">The stock ID to filter comments by</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <param name="pageNumber">Page number (1-based), null for no pagination</param>
    /// <param name="pageSize">Number of items per page, null for no pagination</param>
    /// <returns>Collection of comments for the specified stock, ordered by most recent first</returns>
    Task<IEnumerable<Comment>> GetByStockIdAsync(
        int stockId,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves count of comments for a specific stock
    /// </summary>
    /// <param name="stockId">The stock ID to filter comments by</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Total count of comments for the specified stock</returns>
    Task<int> GetCountByStockIdAsync(int stockId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all comments with optional pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (1-based), null for no pagination</param>
    /// <param name="pageSize">Number of items per page, null for no pagination</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Paginated collection of comments ordered by most recent first</returns>
    Task<IEnumerable<Comment>> GetAllAsync(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
}
