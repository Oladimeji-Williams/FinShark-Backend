using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

/// <summary>
/// Repository implementation for Comment entity
/// Handles all data access operations for comments
/// </summary>
public sealed class CommentRepository : ICommentRepository
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly ILogger<CommentRepository> _logger;

    public CommentRepository(AppDbContext context, ILogger<CommentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Comment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching comment {CommentId}", id);
        return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetByStockIdAsync(
        int stockId,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching comments for stock {StockId}. Page: {PageNumber}, PageSize: {PageSize}",
            stockId, pageNumber, pageSize);

        var query = _context.Comments
            .Where(c => c.StockId == stockId)
            .OrderByDescending(c => c.Created);

        if (pageNumber.HasValue || pageSize.HasValue)
        {
            var resolvedPageNumber = Math.Max(1, pageNumber.GetValueOrDefault(1));
            var resolvedPageSize = Math.Clamp(pageSize.GetValueOrDefault(DefaultPageSize), 1, MaxPageSize);
            var skip = (resolvedPageNumber - 1) * resolvedPageSize;

            return await query
                .Skip(skip)
                .Take(resolvedPageSize)
                .ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return GetAllAsync(null, null, cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetAllAsync(
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all comments. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

        var query = _context.Comments
            .OrderByDescending(c => c.Created)
            .AsQueryable();

        if (pageNumber.HasValue || pageSize.HasValue)
        {
            var resolvedPageNumber = Math.Max(1, pageNumber.GetValueOrDefault(1));
            var resolvedPageSize = Math.Clamp(pageSize.GetValueOrDefault(DefaultPageSize), 1, MaxPageSize);
            var skip = (resolvedPageNumber - 1) * resolvedPageSize;

            return await query
                .Skip(skip)
                .Take(resolvedPageSize)
                .ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Comments.CountAsync(cancellationToken);
    }

    public async Task<int> GetCountByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Counting comments for stock {StockId}", stockId);
        return await _context.Comments.CountAsync(c => c.StockId == stockId, cancellationToken);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding comment {CommentId}", comment.Id);
        await _context.Comments.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating comment {CommentId}", comment.Id);
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting comment {CommentId}", comment.Id);
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
