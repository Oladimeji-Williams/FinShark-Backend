using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.CreateStock;

/// <summary>
/// Handler for creating a new stock
/// Implements CQRS pattern - processes state-changing operations
/// Includes logging for audit trail and debugging
/// </summary>
public sealed class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, int>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<CreateStockCommandHandler> _logger;

    public CreateStockCommandHandler(
        IStockRepository stockRepository,
        ILogger<CreateStockCommandHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(
        CreateStockCommand request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Creating new stock with symbol: {Symbol}, company: {CompanyName}", 
            request.Symbol, request.CompanyName);

        try
        {
            var stock = new Domain.Entities.Stock(
                request.Symbol,
                request.CompanyName,
                request.CurrentPrice
            )
            {
                Industry = request.Industry,
                MarketCap = request.MarketCap
            };

            await _stockRepository.AddAsync(stock);

            _logger.LogInformation("Stock created successfully with ID: {StockId}", stock.Id);

            return stock.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock with symbol: {Symbol}", request.Symbol);
            throw;
        }
    }
}