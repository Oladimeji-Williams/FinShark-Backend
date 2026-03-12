using MediatR;
using FinShark.Application.Dtos;

namespace FinShark.Application.Comments.Queries.GetCommentsByStockId;

/// <summary>
/// Query to get all comments for a specific stock
/// </summary>
public sealed record GetCommentsByStockIdQuery(
    int StockId,
    int? PageNumber = null,
    int? PageSize = null
) : IRequest<IEnumerable<GetCommentResponseDto>>;
