using FinShark.Application.Dtos;
using MediatR;

namespace FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;

/// <summary>
/// Query for retrieving a stock quote from FMP by symbol.
/// </summary>
public sealed record GetStockQuoteFromFmpQuery(string Symbol) : IRequest<GetStockResponseDto>;
