using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;

/// <summary>
/// Query for retrieving a stock quote from FMP by symbol.
/// </summary>
public sealed record GetStockQuoteFromFmpQuery(string Symbol) : IRequest<GetStockResponseDto>;
