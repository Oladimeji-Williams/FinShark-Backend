using FinShark.Domain.Entities;
using FinShark.Domain.Queries;
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
        string? stockSymbol = null,
        int? minRating = null,
        int? maxRating = null,
        string? titleContains = null,
        string? contentContains = null,
        CommentSortBy sortBy = CommentSortBy.Created,
        SortDirection sortDirection = SortDirection.Desc,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching comments for stock {StockId}. Page: {PageNumber}, PageSize: {PageSize}, StockSymbol: {StockSymbol}, MinRating: {MinRating}, MaxRating: {MaxRating}, TitleContains: {TitleContains}, ContentContains: {ContentContains}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            stockId, pageNumber, pageSize, stockSymbol, minRating, maxRating, titleContains, contentContains, sortBy, sortDirection);

        var query = _context.Comments
            .Include(c => c.Stock)
            .Where(c => c.StockId == stockId);

        if (!string.IsNullOrWhiteSpace(stockSymbol))
        {
            var symbol = stockSymbol.Trim();
            query = query.Where(c => EF.Functions.Like(c.Stock.Symbol, $"%{symbol}%"));
        }

        if (minRating.HasValue)
            query = query.Where(c => c.Rating.Value >= minRating.Value);

        if (maxRating.HasValue)
            query = query.Where(c => c.Rating.Value <= maxRating.Value);

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            var title = titleContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{title}%"));
        }

        if (!string.IsNullOrWhiteSpace(contentContains))
        {
            var content = contentContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Content, $"%{content}%"));
        }

        var descending = sortDirection == SortDirection.Desc;
        query = sortBy switch
        {
            CommentSortBy.Rating => descending ? query.OrderByDescending(c => c.Rating.Value) : query.OrderBy(c => c.Rating.Value),
            CommentSortBy.Title => descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            CommentSortBy.Symbol => descending ? query.OrderByDescending(c => c.Stock.Symbol) : query.OrderBy(c => c.Stock.Symbol),
            _ => descending ? query.OrderByDescending(c => EF.Property<DateTime>(c, "Created")) : query.OrderBy(c => EF.Property<DateTime>(c, "Created")),
        };

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
        return GetAllAsync(null, null, null, null, null, null, null, null, CommentSortBy.Created, SortDirection.Desc, cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetAllAsync(
        int? pageNumber = null,
        int? pageSize = null,
        int? stockId = null,
        string? stockSymbol = null,
        int? minRating = null,
        int? maxRating = null,
        string? titleContains = null,
        string? contentContains = null,
        CommentSortBy sortBy = CommentSortBy.Created,
        SortDirection sortDirection = SortDirection.Desc,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all comments. Page: {PageNumber}, PageSize: {PageSize}, StockId: {StockId}, StockSymbol: {StockSymbol}, MinRating: {MinRating}, MaxRating: {MaxRating}, TitleContains: {TitleContains}, ContentContains: {ContentContains}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            pageNumber, pageSize, stockId, stockSymbol, minRating, maxRating, titleContains, contentContains, sortBy, sortDirection);

        var query = _context.Comments
            .Include(c => c.Stock)
            .AsQueryable();

        if (stockId.HasValue)
            query = query.Where(c => c.StockId == stockId.Value);

        if (!string.IsNullOrWhiteSpace(stockSymbol))
        {
            var symbol = stockSymbol.Trim();
            query = query.Where(c => EF.Functions.Like(c.Stock.Symbol, $"%{symbol}%"));
        }

        if (minRating.HasValue)
            query = query.Where(c => c.Rating.Value >= minRating.Value);

        if (maxRating.HasValue)
            query = query.Where(c => c.Rating.Value <= maxRating.Value);

        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            var title = titleContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{title}%"));
        }

        if (!string.IsNullOrWhiteSpace(contentContains))
        {
            var content = contentContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Content, $"%{content}%"));
        }

        var descending = sortDirection == SortDirection.Desc;
        query = sortBy switch
        {
            CommentSortBy.Rating => descending ? query.OrderByDescending(c => c.Rating.Value) : query.OrderBy(c => c.Rating.Value),
            CommentSortBy.Title => descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            CommentSortBy.Symbol => descending ? query.OrderByDescending(c => c.Stock.Symbol) : query.OrderBy(c => c.Stock.Symbol),
            _ => descending ? query.OrderByDescending(c => EF.Property<DateTime>(c, "Created")) : query.OrderBy(c => EF.Property<DateTime>(c, "Created")),
        };
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

    public async Task DeleteAsync(Comment comment, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));

        if (hardDelete)
        {
            _logger.LogInformation("Hard deleting comment {CommentId}", comment.Id);
            _context.Comments.Remove(comment);
        }
        else
        {
            _logger.LogInformation("Soft deleting comment {CommentId}", comment.Id);
            comment.SoftDelete();
            _context.Comments.Update(comment);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
