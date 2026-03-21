using MediatR;

namespace FinShark.Application.Stocks.Commands.AddStockToPortfolio;

/// <summary>
/// Command to add a stock to a user's portfolio by stock symbol.
/// Falls back to Financial Modeling Prep when the stock is not yet in the local database.
/// </summary>
public sealed record AddStockToPortfolioBySymbolCommand(string UserId, string Symbol) : IRequest<bool>;
