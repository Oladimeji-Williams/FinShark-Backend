using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetStocks;

/// <summary>
/// Handler for retrieving all stocks
/// Implements CQRS pattern - processes read-only operations
/// Uses manual mapper for explicit control over DTO construction
/// </summary>
public sealed class GetStocksQueryHandler : IRequestHandler<GetStocksQuery, IEnumerable<GetStockResponseDto>>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetStocksQueryHandler> _logger;

    public GetStocksQueryHandler(
        IStockRepository stockRepository,
        ILogger<GetStocksQueryHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<GetStockResponseDto>> Handle(
        GetStocksQuery request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Retrieving all stocks");

        try
        {
            var stocks = await _stockRepository.GetAllAsync(cancellationToken);
            var stockDtos = StockMapper.ToDtoList(stocks);

            _logger.LogInformation("Retrieved {Count} stocks", stocks.Count());

            return stockDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stocks");
            throw;
        }
    }
}