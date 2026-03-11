using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Repositories;

/// <summary>
/// Repository implementation for Stock entity
/// Handles database operations with logging and error handling
/// </summary>
public sealed class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockRepository> _logger;

    public StockRepository(
        AppDbContext context,
        ILogger<StockRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching stock with ID: {StockId}", id);
            var stock = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            
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

    public async Task DeleteAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        if (stock == null) throw new ArgumentNullException(nameof(stock));

        try
        {
            _logger.LogInformation("Deleting stock with ID: {StockId}", stock.Id);
            _context.Stocks.Remove(stock);
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