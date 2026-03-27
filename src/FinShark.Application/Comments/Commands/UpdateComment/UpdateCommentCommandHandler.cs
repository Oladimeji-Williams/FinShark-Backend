using MediatR;
using FinShark.Domain.Repositories;
using FinShark.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using FinShark.Application.Mappers;
using FinShark.Application.Comments.Commands.UpdateComment;

namespace FinShark.Application.Comments.Commands.UpdateComment;
public sealed class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<UpdateCommentCommandHandler> _logger;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        ILogger<UpdateCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating comment {CommentId}", request.Id);

        // Get existing comment
        var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment == null)
            throw new CommentNotFoundException($"Comment with ID {request.Id} not found");

        if (!request.IsAdmin && !string.Equals(comment.UserId, request.RequestingUserId, StringComparison.Ordinal))
        {
            throw new ForbiddenOperationException("You are not allowed to update this comment.");
        }

        // Update comment via mapper (manual mapping for explicit control)
        CommentMapper.UpdateEntity(comment, request);

        // Save changes
        await _commentRepository.UpdateAsync(comment, cancellationToken);

        _logger.LogInformation("Comment {CommentId} updated successfully", request.Id);

        return true;
    }
}
