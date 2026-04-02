using FinShark.Application.Dtos;
using MediatorFlow.Core.Contracts;

namespace FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;

/// <summary>
/// Query for retrieving the full FMP company profile for a symbol.
/// </summary>
public sealed record GetFullStockQuoteFromFmpQuery(string Symbol) : IRequest<GetFullStockQuoteResponseDto>;
