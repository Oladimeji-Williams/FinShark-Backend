using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Stocks.Queries.GetPortfolioStocks;

public sealed record GetPortfolioStocksQuery(string UserId) : IRequest<IEnumerable<GetStockResponseDto>>;
