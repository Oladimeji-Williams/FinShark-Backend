using FinShark.Application.Dtos;
using FinShark.Domain.Queries;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetStocks;

/// <summary>
/// Query to retrieve stocks with filtering, sorting, and pagination
/// Implements CQRS pattern - responsible for state read operations
/// </summary>
public sealed record GetStocksQuery(StockQueryParameters QueryParameters) : IRequest<PagedResult<GetStockResponseDto>>;
