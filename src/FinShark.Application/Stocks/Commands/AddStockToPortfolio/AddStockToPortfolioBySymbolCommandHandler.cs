using FinShark.Application.Common;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Repositories;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.AddStockToPortfolio;

public sealed class AddStockToPortfolioBySymbolCommandHandler : IRequestHandler<AddStockToPortfolioBySymbolCommand, bool>
{
    private readonly IStockRepository _stockRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IFmpService _fmpService;
    private readonly ILogger<AddStockToPortfolioBySymbolCommandHandler> _logger;

    public AddStockToPortfolioBySymbolCommandHandler(
        IStockRepository stockRepository,
        IPortfolioRepository portfolioRepository,
        IFmpService fmpService,
        ILogger<AddStockToPortfolioBySymbolCommandHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _fmpService = fmpService ?? throw new ArgumentNullException(nameof(fmpService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(AddStockToPortfolioBySymbolCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("UserId cannot be empty", nameof(request.UserId));
        if (string.IsNullOrWhiteSpace(request.Symbol)) throw new ArgumentException("Symbol cannot be empty", nameof(request.Symbol));

        var normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();
        _logger.LogInformation("Adding stock to portfolio for user {UserId} by symbol {Symbol}", request.UserId, normalizedSymbol);

        var stock = await _stockRepository.GetBySymbolAsync(normalizedSymbol, cancellationToken);
        if (stock is null)
        {
            _logger.LogInformation("Stock symbol {Symbol} not found locally. Fetching from FMP.", normalizedSymbol);
            var fmpStock = await _fmpService.GetStockQuoteAsync(normalizedSymbol, cancellationToken);
            if (fmpStock is null)
            {
                _logger.LogWarning("Stock symbol {Symbol} not found in FMP.", normalizedSymbol);
                throw new StockNotFoundException($"Stock symbol '{normalizedSymbol}' not found in FMP.");
            }

            stock = await _stockRepository.GetBySymbolAsync(fmpStock.Symbol, cancellationToken);
            if (stock is null)
            {
                await _stockRepository.AddAsync(fmpStock, cancellationToken);
                stock = await _stockRepository.GetBySymbolAsync(fmpStock.Symbol, cancellationToken);
            }

            if (stock is null)
            {
                _logger.LogError("Failed to persist stock symbol {Symbol} after fetching from FMP.", normalizedSymbol);
                throw new InvalidOperationException($"Unable to save fetched stock '{normalizedSymbol}' to local database.");
            }
        }

        _logger.LogInformation("Stock ID {StockId} for symbol {Symbol} is resolved locally. Adding to portfolio.", stock.Id, stock.Symbol);
        var added = await _portfolioRepository.AddStockToPortfolioAsync(request.UserId, stock.Id, cancellationToken);
        if (!added)
        {
            _logger.LogInformation("Stock {StockId} was not added to portfolio for user {UserId} (already exists or failure).", stock.Id, request.UserId);
        }

        return added;
    }
}
