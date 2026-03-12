using MediatR;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Comments.Queries.GetAllComments;
public sealed class GetAllCommentsQueryHandler : IRequestHandler<GetAllCommentsQuery, IEnumerable<GetCommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<GetAllCommentsQueryHandler> _logger;

    public GetAllCommentsQueryHandler(
        ICommentRepository commentRepository,
        ILogger<GetAllCommentsQueryHandler> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetCommentResponseDto>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all comments. Page: {PageNumber}, PageSize: {PageSize}",
            request.PageNumber, request.PageSize);

        var comments = await _commentRepository.GetAllAsync(request.PageNumber, request.PageSize);

        _logger.LogInformation("Retrieved {CommentCount} comments", comments.Count());

        return CommentMapper.ToDtoList(comments);
    }
}
