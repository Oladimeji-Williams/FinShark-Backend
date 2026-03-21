using MediatR;

namespace FinShark.Application.Comments.Commands.DeleteComment;

/// <summary>
/// Command to delete a comment
/// </summary>
public sealed record DeleteCommentCommand(int Id, bool HardDelete = false) : IRequest<bool>;
