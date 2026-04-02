using FinShark.Domain.Repositories;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.AddStockToPortfolio;

public sealed class AddStockToPortfolioCommandHandler : IRequestHandler<AddStockToPortfolioCommand, bool>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<AddStockToPortfolioCommandHandler> _logger;

    public AddStockToPortfolioCommandHandler(
        IPortfolioRepository portfolioRepository,
        ILogger<AddStockToPortfolioCommandHandler> logger)
    {
        _portfolioRepository = portfolioRepository ?? throw new ArgumentNullException(nameof(portfolioRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(AddStockToPortfolioCommand request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("UserId cannot be empty", nameof(request.UserId));

        _logger.LogInformation("Adding stock {StockId} to portfolio for user {UserId}", request.StockId, request.UserId);

        var result = await _portfolioRepository.AddStockToPortfolioAsync(request.UserId, request.StockId, cancellationToken);

        if (!result)
        {
            _logger.LogInformation("Did not add stock {StockId} to portfolio for user {UserId} (might already exist or stock missing)", request.StockId, request.UserId);
        }

        return result;
    }
}
