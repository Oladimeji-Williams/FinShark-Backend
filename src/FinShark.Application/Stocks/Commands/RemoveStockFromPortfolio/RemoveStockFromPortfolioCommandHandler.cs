using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.RemoveStockFromPortfolio;

public sealed class RemoveStockFromPortfolioCommandHandler : IRequestHandler<RemoveStockFromPortfolioCommand, bool>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<RemoveStockFromPortfolioCommandHandler> _logger;

    public RemoveStockFromPortfolioCommandHandler(
        IPortfolioRepository portfolioRepository,
        ILogger<RemoveStockFromPortfolioCommandHandler> logger)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(RemoveStockFromPortfolioCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("UserId cannot be empty", nameof(request.UserId));

        _logger.LogInformation("Removing stock {StockId} from portfolio for user {UserId}", request.StockId, request.UserId);
        var result = await _portfolioRepository.RemoveStockFromPortfolioAsync(request.UserId, request.StockId, cancellationToken);

        if (!result)
        {
            _logger.LogInformation("Stock {StockId} was not present in portfolio for user {UserId}", request.StockId, request.UserId);
        }

        return result;
    }
}
