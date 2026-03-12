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

    public async Task<IEnumerable<Comment>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching comments for stock {StockId}", stockId);
        return await _context.Comments
            .Where(c => c.StockId == stockId)
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);
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

        var query = _context.Comments.AsQueryable();

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var skip = (pageNumber.Value - 1) * pageSize.Value;
            return await query
                .OrderByDescending(c => c.Created)
                .Skip(skip)
                .Take(pageSize.Value)
                .ToListAsync(cancellationToken);
        }

        return await query
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Comments.CountAsync(cancellationToken);
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
