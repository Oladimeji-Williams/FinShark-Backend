using MediatorFlow.Core.Contracts;
using FinShark.Application.Dtos;

namespace FinShark.Application.Stocks.Queries.GetStockById;

/// <summary>
/// Query to retrieve a single stock by ID
/// </summary>
public sealed record GetStockByIdQuery(int Id) : IRequest<GetStockResponseDto>;
