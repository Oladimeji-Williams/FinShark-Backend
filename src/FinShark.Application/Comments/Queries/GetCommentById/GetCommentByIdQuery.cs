using MediatorFlow.Core.Contracts;
using FinShark.Application.Dtos;

namespace FinShark.Application.Comments.Queries.GetCommentById;

/// <summary>
/// Query to get a comment by ID
/// </summary>
public sealed record GetCommentByIdQuery(int Id) : IRequest<GetCommentResponseDto>;
