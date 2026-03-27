using FinShark.Domain.Entities;
using FinShark.Domain.Queries;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

/// <summary>
/// Repository implementation for Stock entity
/// Handles database operations with logging and error handling
/// </summary>
public sealed class StockRepository(
    AppDbContext appDbContext,
    ILogger<StockRepository> logger) : IStockRepository
{
    private readonly AppDbContext _context = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
    private readonly ILogger<StockRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all stocks from database");
            var stocks = await _context.Stocks.ToListAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} stocks", stocks.Count);
            return stocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stocks");
            throw;
        }
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Counting stocks in database");
            return await _context.Stocks.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting stocks");
            throw;
        }
    }

    public async Task<IEnumerable<Stock>> GetAllWithCommentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all stocks with comments from database");
            var stocks = await _context.Stocks
                .Include(s => s.Comments)
                .ToListAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} stocks with comments", stocks.Count);
            return stocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stocks with comments");
            throw;
        }
    }

    public async Task<IEnumerable<Stock>> GetAllWithCommentsAsync(StockQueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        if (queryParameters == null) throw new ArgumentNullException(nameof(queryParameters));

        try
        {
            _logger.LogInformation("Fetching stocks with filters from database");

            IQueryable<Stock> query = _context.Stocks
                .Include(s => s.Comments);

            query = ApplyFilters(query, queryParameters);

            query = ApplySorting(query, queryParameters);

            if (queryParameters.PageNumber.HasValue || queryParameters.PageSize.HasValue)
            {
                var pageNumber = queryParameters.PageNumber.GetValueOrDefault(1);
                var pageSize = queryParameters.PageSize.GetValueOrDefault(20);

                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var stocks = await query.ToListAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} stocks with filters", stocks.Count);
            return stocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stocks with filters");
            throw;
        }
    }

    public async Task<int> GetCountAsync(StockQueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        if (queryParameters == null) throw new ArgumentNullException(nameof(queryParameters));

        try
        {
            _logger.LogInformation("Counting stocks with filters from database");

            IQueryable<Stock> query = _context.Stocks;
            query = ApplyFilters(query, queryParameters);

            var count = await query.CountAsync(cancellationToken);
            _logger.LogInformation("Counted {Count} stocks with filters", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting stocks with filters");
            throw;
        }
    }

    private static IQueryable<Stock> ApplyFilters(IQueryable<Stock> query, StockQueryParameters queryParameters)
    {
        if (!string.IsNullOrWhiteSpace(queryParameters.Symbol))
        {
            var symbol = queryParameters.Symbol.Trim();
            query = query.Where(s => EF.Functions.Like(s.Symbol, $"%{symbol}%"));
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.CompanyName))
        {
            var companyName = queryParameters.CompanyName.Trim();
            query = query.Where(s => EF.Functions.Like(s.CompanyName, $"%{companyName}%"));
        }

        if (queryParameters.Sector.HasValue)
        {
            query = query.Where(s => s.Sector == queryParameters.Sector.Value);
        }

        if (queryParameters.MinPrice.HasValue)
        {
            query = query.Where(s => s.CurrentPrice >= queryParameters.MinPrice.Value);
        }

        if (queryParameters.MaxPrice.HasValue)
        {
            query = query.Where(s => s.CurrentPrice <= queryParameters.MaxPrice.Value);
        }

        if (queryParameters.MinMarketCap.HasValue)
        {
            query = query.Where(s => s.MarketCap >= queryParameters.MinMarketCap.Value);
        }

        if (queryParameters.MaxMarketCap.HasValue)
        {
            query = query.Where(s => s.MarketCap <= queryParameters.MaxMarketCap.Value);
        }

        return query;
    }

    private static IQueryable<Stock> ApplySorting(IQueryable<Stock> query, StockQueryParameters queryParameters)
    {
        var descending = queryParameters.SortDirection == SortDirection.Desc;

        return queryParameters.SortBy switch
        {
            StockSortBy.CompanyName => descending ? query.OrderByDescending(s => s.CompanyName) : query.OrderBy(s => s.CompanyName),
            StockSortBy.CurrentPrice => descending ? query.OrderByDescending(s => s.CurrentPrice) : query.OrderBy(s => s.CurrentPrice),
            StockSortBy.MarketCap => descending ? query.OrderByDescending(s => s.MarketCap) : query.OrderBy(s => s.MarketCap),
            StockSortBy.Created => descending ? query.OrderByDescending(s => s.Created) : query.OrderBy(s => s.Created),
            _ => descending ? query.OrderByDescending(s => s.Symbol) : query.OrderBy(s => s.Symbol)
        };
    }

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching stock with ID: {StockId}", id);
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (stock is null)
            {
                _logger.LogWarning("Stock with ID {StockId} not found", id);
            }

            return stock;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock with ID: {StockId}", id);
            throw;
        }
    }

    public async Task<Stock?> GetByIdWithCommentsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching stock with ID: {StockId} including comments", id);
            var stock = await _context.Stocks
                .Include(s => s.Comments)
                .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (stock is null)
            {
                _logger.LogWarning("Stock with ID {StockId} not found", id);
            }

            return stock;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock with ID: {StockId} including comments", id);
            throw;
        }
    }

    public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        try
        {
            _logger.LogInformation("Fetching stock with symbol: {Symbol}", symbol);
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol, cancellationToken);
            
            if (stock is null)
            {
                _logger.LogDebug("Stock with symbol {Symbol} not found", symbol);
            }

            return stock;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock with symbol: {Symbol}", symbol);
            throw;
        }
    }

    public async Task AddAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        if (stock == null) throw new ArgumentNullException(nameof(stock));

        try
        {
            _logger.LogInformation("Adding new stock: {Symbol}", stock.Symbol);
            await _context.Stocks.AddAsync(stock, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock added successfully with ID: {StockId}", stock.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stock: {Symbol}", stock.Symbol);
            throw;
        }
    }

    public async Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        if (stock == null) throw new ArgumentNullException(nameof(stock));

        try
        {
            _logger.LogInformation("Updating stock with ID: {StockId}, Symbol: {Symbol}", stock.Id, stock.Symbol);
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock updated successfully with ID: {StockId}", stock.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock with ID: {StockId}", stock.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Stock stock, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        if (stock == null) throw new ArgumentNullException(nameof(stock));

        try
        {
            if (hardDelete)
            {
                _logger.LogInformation("Hard deleting stock with ID: {StockId}", stock.Id);
                _context.Stocks.Remove(stock);
            }
            else
            {
                _logger.LogInformation("Soft deleting stock with ID: {StockId}", stock.Id);
                stock.SoftDelete();
                _context.Stocks.Update(stock);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stock deleted successfully with ID: {StockId}", stock.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock with ID: {StockId}", stock.Id);
            throw;
        }
    }
}
