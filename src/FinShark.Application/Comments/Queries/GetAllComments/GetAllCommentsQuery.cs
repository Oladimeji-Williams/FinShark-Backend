using MediatorFlow.Core.Contracts;
using FinShark.Application.Dtos;

namespace FinShark.Application.Comments.Queries.GetAllComments;

/// <summary>
/// Query to get all comments
/// </summary>
public sealed record GetAllCommentsQuery(CommentQueryRequestDto QueryParameters) : IRequest<PagedResult<GetCommentResponseDto>>;
