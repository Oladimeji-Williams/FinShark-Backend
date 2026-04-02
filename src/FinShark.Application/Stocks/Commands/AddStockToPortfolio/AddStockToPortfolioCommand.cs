using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Stocks.Commands.AddStockToPortfolio;

/// <summary>
/// Command to add a stock to a user's portfolio by userId and stockId
/// </summary>
public sealed record AddStockToPortfolioCommand(string UserId, int StockId) : IRequest<bool>;
