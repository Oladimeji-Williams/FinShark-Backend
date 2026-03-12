using MediatR;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using FinShark.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using FinShark.Application.Comments.Queries.GetCommentById;

namespace FinShark.Application.Comments.Queries.GetCommentById;
public sealed class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, GetCommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<GetCommentByIdQueryHandler> _logger;

    public GetCommentByIdQueryHandler(
        ICommentRepository commentRepository,
        ILogger<GetCommentByIdQueryHandler> logger)
    {
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task<GetCommentResponseDto> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting comment {CommentId}", request.Id);

        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
            throw new CommentNotFoundException($"Comment with ID {request.Id} not found");

        _logger.LogInformation("Comment {CommentId} retrieved successfully", request.Id);

        return CommentMapper.ToDto(comment);
    }
}
