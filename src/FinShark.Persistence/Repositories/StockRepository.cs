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
    ILogger<StockRepository> logger) : IStockRepository, IPortfolioRepository
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
            StockSortBy.Created => descending ? query.OrderByDescending(s => EF.Property<DateTime>(s, "Created")) : query.OrderBy(s => EF.Property<DateTime>(s, "Created")),
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

    public async Task<IEnumerable<Stock>> GetPortfolioAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation("Fetching portfolio stocks for user: {UserId}", userId);

            var portfolioStocks = await _context.PortfolioItems
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Include(p => p.Stock)
                .ThenInclude(s => s.Comments)
                .Select(p => p.Stock!)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} portfolio stocks for user: {UserId}", portfolioStocks.Count, userId);
            return portfolioStocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portfolio for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddStockToPortfolioAsync(string userId, int stockId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation("Adding stock {StockId} to portfolio for user: {UserId}", stockId, userId);

            var existing = await _context.PortfolioItems
                .FirstOrDefaultAsync(pi => pi.UserId == userId && pi.StockId == stockId, cancellationToken);

            if (existing != null)
            {
                if (!existing.IsDeleted)
                {
                    _logger.LogInformation("Stock {StockId} is already in portfolio for user: {UserId}", stockId, userId);
                    return false;
                }

                existing.Restore();
                _context.PortfolioItems.Update(existing);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Restored previously deleted portfolio item for stock {StockId} and user: {UserId}", stockId, userId);
                return true;
            }

            var stock = await _context.Stocks.FindAsync(new object[] { stockId }, cancellationToken);
            if (stock is null)
            {
                _logger.LogWarning("Stock {StockId} does not exist", stockId);
                return false;
            }

            await _context.PortfolioItems.AddAsync(new PortfolioItem { UserId = userId, StockId = stockId }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock {StockId} added to portfolio for user: {UserId}", stockId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stock {StockId} to portfolio for user: {UserId}", stockId, userId);
            throw;
        }
    }

    public async Task<bool> RemoveStockFromPortfolioAsync(string userId, int stockId, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation("Removing stock {StockId} from portfolio for user: {UserId}", stockId, userId);

            var portfolioItem = await _context.PortfolioItems
                .SingleOrDefaultAsync(pi => pi.UserId == userId && pi.StockId == stockId, cancellationToken);

            if (portfolioItem is null)
            {
                _logger.LogInformation("Stock {StockId} not found in portfolio for user: {UserId}", stockId, userId);
                return false;
            }

            if (hardDelete)
            {
                _logger.LogInformation("Hard deleting portfolio item for stock {StockId}, user {UserId}", stockId, userId);
                _context.PortfolioItems.Remove(portfolioItem);
            }
            else
            {
                _logger.LogInformation("Soft deleting portfolio item for stock {StockId}, user {UserId}", stockId, userId);
                portfolioItem.SoftDelete();
                _context.PortfolioItems.Update(portfolioItem);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock {StockId} removed from portfolio for user: {UserId}", stockId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing stock {StockId} from portfolio for user: {UserId}", stockId, userId);
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

