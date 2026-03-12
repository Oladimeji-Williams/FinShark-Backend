using MediatR;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Command to update an existing comment - supports partial updates with optional fields
/// </summary>
public sealed record UpdateCommentCommand(
    int Id,
    string? Title,
    string? Content,
    Rating? Rating
) : IRequest<bool>;
