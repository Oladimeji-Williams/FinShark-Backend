using FinShark.Domain.Entities;
using FinShark.Domain.Queries;
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
    /// Retrieves comments with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="queryParameters">Filtering, sorting, and pagination criteria.</param>
    Task<IEnumerable<Comment>> GetAllAsync(CommentQueryParameters queryParameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the total count of comments for a filtered query.
    /// </summary>
    /// <param name="queryParameters">Filtering criteria.</param>
    Task<int> GetCountAsync(CommentQueryParameters queryParameters, CancellationToken cancellationToken = default);
}
