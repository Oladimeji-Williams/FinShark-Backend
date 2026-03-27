using FinShark.Application.Dtos;
using FinShark.Application.Common;
using FinShark.Application.Mappers;
using FinShark.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;

public sealed class GetFullStockQuoteFromFmpQueryHandler : IRequestHandler<GetFullStockQuoteFromFmpQuery, GetFullStockQuoteResponseDto>
{
    private readonly IFmpService _fmpService;
    private readonly ILogger<GetFullStockQuoteFromFmpQueryHandler> _logger;

    public GetFullStockQuoteFromFmpQueryHandler(IFmpService fmpService, ILogger<GetFullStockQuoteFromFmpQueryHandler> logger)
    {
        _fmpService = fmpService ?? throw new ArgumentNullException(nameof(fmpService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetFullStockQuoteResponseDto> Handle(GetFullStockQuoteFromFmpQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            throw new ArgumentException("Symbol must be provided.", nameof(request.Symbol));
        }

        _logger.LogInformation("Handling full FMP quote query for symbol: {Symbol}", request.Symbol);

        var fmpProfile = await _fmpService.GetFullStockProfileAsync(request.Symbol, cancellationToken);
        if (fmpProfile is null)
        {
            _logger.LogWarning("FMP returned no stock data for symbol: {Symbol}", request.Symbol);
            throw new StockNotFoundException($"Stock symbol '{request.Symbol}' not found in FMP.");
        }

        _logger.LogInformation("Retrieved full FMP stock profile for symbol: {Symbol}", request.Symbol);

        return FmpStockProfileMapper.ToDto(fmpProfile);
    }
}
