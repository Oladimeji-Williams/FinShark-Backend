using MediatR;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Application.Common;
using FinShark.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Comments.Queries.GetAllComments;
public sealed class GetAllCommentsQueryHandler : IRequestHandler<GetAllCommentsQuery, PagedResult<GetCommentResponseDto>>
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

    public async Task<PagedResult<GetCommentResponseDto>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all comments. Page: {PageNumber}, PageSize: {PageSize}",
            request.PageNumber, request.PageSize);

        var comments = await _commentRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.StockId,
            request.StockSymbol,
            request.MinRating,
            request.MaxRating,
            request.TitleContains,
            request.ContentContains,
            request.SortBy,
            request.SortDirection,
            cancellationToken);
        var commentDtos = CommentMapper.ToDtoList(comments).ToList();
        var isPaged = request.PageNumber.HasValue || request.PageSize.HasValue;
        var totalCount = isPaged
            ? await _commentRepository.GetCountAsync(cancellationToken)
            : commentDtos.Count;
        var pagination = PaginationHelper.Build(totalCount, request.PageNumber, request.PageSize);

        _logger.LogInformation("Retrieved {CommentCount} comments", commentDtos.Count);

        return new PagedResult<GetCommentResponseDto>
        {
            Items = commentDtos,
            Pagination = pagination
        };
    }
}
