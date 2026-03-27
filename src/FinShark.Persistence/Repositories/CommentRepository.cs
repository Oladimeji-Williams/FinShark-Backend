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

    public Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return GetAllAsync(new CommentQueryParameters(), cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetAllAsync(CommentQueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        _logger.LogInformation("Fetching all comments. Page: {PageNumber}, PageSize: {PageSize}, StockId: {StockId}, StockSymbol: {StockSymbol}, MinRating: {MinRating}, MaxRating: {MaxRating}, TitleContains: {TitleContains}, ContentContains: {ContentContains}, SortBy: {SortBy}, SortDirection: {SortDirection}",
            queryParameters.PageNumber,
            queryParameters.PageSize,
            queryParameters.StockId,
            queryParameters.StockSymbol,
            queryParameters.MinRating,
            queryParameters.MaxRating,
            queryParameters.TitleContains,
            queryParameters.ContentContains,
            queryParameters.SortBy,
            queryParameters.SortDirection);

        var query = BuildQuery(queryParameters);

        if (queryParameters.PageNumber.HasValue || queryParameters.PageSize.HasValue)
        {
            var resolvedPageNumber = Math.Max(1, queryParameters.PageNumber.GetValueOrDefault(1));
            var resolvedPageSize = Math.Clamp(queryParameters.PageSize.GetValueOrDefault(DefaultPageSize), 1, MaxPageSize);
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

    public async Task<int> GetCountAsync(CommentQueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        _logger.LogInformation("Counting comments for filtered query");
        return await BuildQuery(queryParameters).CountAsync(cancellationToken);
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

    private IQueryable<Comment> BuildQuery(CommentQueryParameters queryParameters)
    {
        var query = _context.Comments
            .Include(c => c.Stock)
            .AsQueryable();

        if (queryParameters.StockId.HasValue)
        {
            query = query.Where(c => c.StockId == queryParameters.StockId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.StockSymbol))
        {
            var symbol = queryParameters.StockSymbol.Trim();
            query = query.Where(c => EF.Functions.Like(c.Stock.Symbol, $"%{symbol}%"));
        }

        if (queryParameters.MinRating.HasValue)
        {
            query = query.Where(c => c.Rating.Value >= queryParameters.MinRating.Value);
        }

        if (queryParameters.MaxRating.HasValue)
        {
            query = query.Where(c => c.Rating.Value <= queryParameters.MaxRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.TitleContains))
        {
            var title = queryParameters.TitleContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{title}%"));
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.ContentContains))
        {
            var content = queryParameters.ContentContains.Trim();
            query = query.Where(c => EF.Functions.Like(c.Content, $"%{content}%"));
        }

        var descending = queryParameters.SortDirection == SortDirection.Desc;

        return queryParameters.SortBy switch
        {
            CommentSortBy.Rating => descending ? query.OrderByDescending(c => c.Rating.Value) : query.OrderBy(c => c.Rating.Value),
            CommentSortBy.Title => descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            CommentSortBy.Symbol => descending ? query.OrderByDescending(c => c.Stock.Symbol) : query.OrderBy(c => c.Stock.Symbol),
            _ => descending ? query.OrderByDescending(c => c.Created) : query.OrderBy(c => c.Created),
        };
    }
}
