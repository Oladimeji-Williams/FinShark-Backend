using MediatR;
using FinShark.Application.Dtos;
using FinShark.Domain.Queries;

namespace FinShark.Application.Comments.Queries.GetAllComments;

/// <summary>
/// Query to get all comments
/// </summary>
public sealed record GetAllCommentsQuery(
    int? PageNumber = null,
    int? PageSize = null,
    int? StockId = null,
    string? StockSymbol = null,
    int? MinRating = null,
    int? MaxRating = null,
    string? TitleContains = null,
    string? ContentContains = null,
    CommentSortBy SortBy = CommentSortBy.Created,
    SortDirection SortDirection = SortDirection.Desc
) : IRequest<PagedResult<GetCommentResponseDto>>;
