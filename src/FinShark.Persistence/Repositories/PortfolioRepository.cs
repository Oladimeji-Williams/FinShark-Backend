using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

public sealed class PortfolioRepository(
    AppDbContext appDbContext,
    ILogger<PortfolioRepository> logger) : IPortfolioRepository
{
    private readonly AppDbContext _context = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
    private readonly ILogger<PortfolioRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                .IgnoreQueryFilters()
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
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(pi => pi.UserId == userId && pi.StockId == stockId, cancellationToken);

            if (portfolioItem is null || portfolioItem.IsDeleted && !hardDelete)
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
}
