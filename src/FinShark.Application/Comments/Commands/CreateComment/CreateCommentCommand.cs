using MediatR;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Comments.Commands.CreateComment;

/// <summary>
/// Command to create a new comment
/// </summary>
public sealed record CreateCommentCommand(
    int StockId,
    string Title,
    string Content,
    Rating Rating
) : IRequest<int>;
