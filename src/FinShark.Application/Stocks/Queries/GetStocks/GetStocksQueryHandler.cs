using FinShark.Application.Dtos;
using FinShark.Application.Common;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetStocks;

/// <summary>
/// Handler for retrieving stocks
/// Implements CQRS pattern - processes read-only operations
/// Uses manual mapper for explicit control over DTO construction
/// </summary>
public sealed class GetStocksQueryHandler : IRequestHandler<GetStocksQuery, PagedResult<GetStockResponseDto>>
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

    public async Task<PagedResult<GetStockResponseDto>> Handle(
        GetStocksQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.QueryParameters);

        _logger.LogInformation("Retrieving stocks with filters");

        try
        {
            var queryParameters = StockQueryParametersMapper.ToDomain(request.QueryParameters);
            var stocks = await _stockRepository.GetAllWithCommentsAsync(queryParameters, cancellationToken);
            var stockDtos = StockMapper.ToDtoList(stocks).ToList();
            var isPaged = request.QueryParameters.PageNumber.HasValue || request.QueryParameters.PageSize.HasValue;
            var totalCount = isPaged
                ? await _stockRepository.GetCountAsync(queryParameters, cancellationToken)
                : stockDtos.Count;
            var pagination = PaginationHelper.Build(
                totalCount,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize);

            _logger.LogInformation("Retrieved {Count} stocks", stockDtos.Count);

            return new PagedResult<GetStockResponseDto>
            {
                Items = stockDtos,
                Pagination = pagination
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stocks");
            throw;
        }
    }

}
