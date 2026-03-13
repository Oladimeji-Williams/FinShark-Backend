using MediatR;
using FinShark.Application.Dtos;

namespace FinShark.Application.Comments.Queries.GetAllComments;

/// <summary>
/// Query to get all comments
/// </summary>
public sealed record GetAllCommentsQuery(
    int? PageNumber = null,
    int? PageSize = null
) : IRequest<PagedResult<GetCommentResponseDto>>;
