using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;

/// <summary>
/// Handler for GetStockQuoteFromFmpQuery.
/// Uses IFMPService to resolve stock data from Financial Modeling Prep.
/// </summary>
public sealed class GetStockQuoteFromFmpQueryHandler : IRequestHandler<GetStockQuoteFromFmpQuery, GetStockResponseDto>
{
    private readonly IFMPService _fmpService;
    private readonly ILogger<GetStockQuoteFromFmpQueryHandler> _logger;

    public GetStockQuoteFromFmpQueryHandler(
        IFMPService fmpService,
        ILogger<GetStockQuoteFromFmpQueryHandler> logger)
    {
        _fmpService = fmpService ?? throw new ArgumentNullException(nameof(fmpService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetStockResponseDto> Handle(GetStockQuoteFromFmpQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            throw new ArgumentException("Symbol must be provided.", nameof(request.Symbol));
        }

        _logger.LogInformation("Handling FMP quote query for symbol: {Symbol}", request.Symbol);

        var stock = await _fmpService.GetStockQuoteAsync(request.Symbol, cancellationToken);
        if (stock == null)
        {
            _logger.LogWarning("FMP returned no stock data for symbol: {Symbol}", request.Symbol);
            throw new StockNotFoundException($"Stock symbol '{request.Symbol}' not found in FMP.");
        }

        var dto = StockMapper.ToDto(stock);
        _logger.LogInformation("Mapped FMP stock quote for symbol {Symbol} to GetStockResponseDto", stock.Symbol);
        return dto;
    }
}
