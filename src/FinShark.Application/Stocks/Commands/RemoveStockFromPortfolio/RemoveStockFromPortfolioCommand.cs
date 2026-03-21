using MediatR;

namespace FinShark.Application.Stocks.Commands.RemoveStockFromPortfolio;

public sealed record RemoveStockFromPortfolioCommand(string UserId, int StockId, bool HardDelete = false) : IRequest<bool>;
