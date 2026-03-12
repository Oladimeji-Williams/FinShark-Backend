using MediatR;
using FinShark.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.UpdateStock;

/// <summary>
/// Handler for UpdateStockCommand
/// Updates an existing stock in the database
/// </summary>
public sealed class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, bool>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(
        IStockRepository stockRepository,
        ILogger<UpdateStockCommandHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Updating stock with ID: {StockId}, Symbol: {Symbol}", request.Id, request.Symbol);

        // Get the existing stock
        var existingStock = await _stockRepository.GetByIdAsync(request.Id, cancellationToken);

        if (existingStock == null)
        {
            _logger.LogWarning("Stock not found for update with ID: {StockId}", request.Id);
            throw new KeyNotFoundException($"Stock with ID {request.Id} not found.");
        }

        try
        {
            // Update entity directly from command (supports partial updates)
            existingStock.Update(
                symbol: request.Symbol,
                companyName: request.CompanyName,
                currentPrice: request.CurrentPrice,
                industry: request.Industry,
                marketCap: request.MarketCap
            );

            // Persist changes
            await _stockRepository.UpdateAsync(existingStock, cancellationToken);

            _logger.LogInformation("Successfully updated stock with ID: {StockId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock with ID: {StockId}", request.Id);
            throw;
        }
    }
}
