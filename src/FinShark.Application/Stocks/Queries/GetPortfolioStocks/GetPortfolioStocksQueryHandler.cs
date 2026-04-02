using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Queries.GetPortfolioStocks;

public sealed class GetPortfolioStocksQueryHandler : IRequestHandler<GetPortfolioStocksQuery, IEnumerable<GetStockResponseDto>>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<GetPortfolioStocksQueryHandler> _logger;

    public GetPortfolioStocksQueryHandler(
        IPortfolioRepository portfolioRepository,
        ILogger<GetPortfolioStocksQueryHandler> logger)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<GetStockResponseDto>> Handle(GetPortfolioStocksQuery request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("UserId cannot be empty", nameof(request.UserId));

        _logger.LogInformation("Retrieving portfolio for user {UserId}", request.UserId);

        var portfolio = await _portfolioRepository.GetPortfolioAsync(request.UserId, cancellationToken);
        return StockMapper.ToDtoList(portfolio);
    }
}
