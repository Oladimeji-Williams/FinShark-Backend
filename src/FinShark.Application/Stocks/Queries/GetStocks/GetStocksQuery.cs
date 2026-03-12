using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetStocks;

/// <summary>
/// Query to retrieve all stocks
/// Implements CQRS pattern - responsible for state read operations
/// </summary>
public sealed record GetStocksQuery() : IRequest<IEnumerable<GetStockResponseDto>>;