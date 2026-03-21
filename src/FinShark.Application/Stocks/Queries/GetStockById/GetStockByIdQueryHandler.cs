using MediatR;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetStockById;

/// <summary>
/// Handler for GetStockByIdQuery
/// Retrieves a single stock by ID from the database
/// </summary>
public sealed class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, GetStockResponseDto>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetStockByIdQueryHandler> _logger;

    public GetStockByIdQueryHandler(
        IStockRepository stockRepository,
        ILogger<GetStockByIdQueryHandler> logger)
    {
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetStockResponseDto> Handle(GetStockByIdQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Retrieving stock with ID: {StockId}", request.Id);

        var stock = await _stockRepository.GetByIdWithCommentsAsync(request.Id, cancellationToken);

        if (stock == null)
        {
            _logger.LogWarning("Stock not found with ID: {StockId}", request.Id);
            throw new StockNotFoundException($"Stock with ID {request.Id} not found.");
        }

        var dto = StockMapper.ToDto(stock);
        _logger.LogInformation("Successfully retrieved stock: {Symbol}", stock.Symbol);

        return dto;
    }
}
