using MediatR;
using FinShark.Application.Dtos;
using FinShark.Domain.Queries;

namespace FinShark.Application.Comments.Queries.GetCommentsByStockId;

/// <summary>
/// Query to get all comments for a specific stock
/// </summary>
public sealed record GetCommentsByStockIdQuery(
    int StockId,
    int? PageNumber = null,
    int? PageSize = null,
    string? StockSymbol = null,
    int? MinRating = null,
    int? MaxRating = null,
    string? TitleContains = null,
    string? ContentContains = null,
    CommentSortBy SortBy = CommentSortBy.Created,
    SortDirection SortDirection = SortDirection.Desc
) : IRequest<PagedResult<GetCommentResponseDto>>;
