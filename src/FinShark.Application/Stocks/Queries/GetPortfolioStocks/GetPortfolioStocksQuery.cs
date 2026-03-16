using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetPortfolioStocks;

public sealed record GetPortfolioStocksQuery(string UserId) : IRequest<IEnumerable<GetStockResponseDto>>;
