using MediatR;
using FinShark.Domain.Repositories;
using FinShark.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using FinShark.Application.Comments.Commands.DeleteComment;

namespace FinShark.Application.Comments.Commands.DeleteComment;
public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    public DeleteCommentCommandHandler(
        ICommentRepository commentRepository,
        ILogger<DeleteCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting comment {CommentId}", request.Id);

        // Get comment
        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
            throw new CommentNotFoundException($"Comment with ID {request.Id} not found");

        // Delete comment
        await _commentRepository.DeleteAsync(comment);

        _logger.LogInformation("Comment {CommentId} deleted successfully", request.Id);

        return true;
    }
}
