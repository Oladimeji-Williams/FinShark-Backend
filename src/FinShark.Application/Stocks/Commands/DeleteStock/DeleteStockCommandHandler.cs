using MediatR;
using FinShark.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.DeleteStock;

/// <summary>
/// Handler for DeleteStockCommand
/// Deletes a stock from the database
/// </summary>
public sealed class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, bool>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<DeleteStockCommandHandler> _logger;

    public DeleteStockCommandHandler(
        IStockRepository stockRepository,
        ILogger<DeleteStockCommandHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Deleting stock with ID: {StockId}", request.Id);

        // Get the existing stock
        var existingStock = await _stockRepository.GetByIdAsync(request.Id, cancellationToken);

        if (existingStock == null)
        {
            _logger.LogWarning("Stock not found for deletion with ID: {StockId}", request.Id);
            throw new KeyNotFoundException($"Stock with ID {request.Id} not found.");
        }

        try
        {
            // Delete the stock
            await _stockRepository.DeleteAsync(existingStock, cancellationToken);

            _logger.LogInformation("Successfully deleted stock with ID: {StockId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock with ID: {StockId}", request.Id);
            throw;
        }
    }
}
