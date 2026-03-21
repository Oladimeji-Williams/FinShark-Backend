using MediatR;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Application.Common;
using FinShark.Domain.Repositories;
using FinShark.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using FinShark.Application.Comments.Queries.GetCommentsByStockId;

namespace FinShark.Application.Comments.Queries.GetCommentsByStockId;
public sealed class GetCommentsByStockIdQueryHandler : IRequestHandler<GetCommentsByStockIdQuery, PagedResult<GetCommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetCommentsByStockIdQueryHandler> _logger;

    public GetCommentsByStockIdQueryHandler(
        ICommentRepository commentRepository,
        IStockRepository stockRepository,
        ILogger<GetCommentsByStockIdQueryHandler> logger)
    {
        _commentRepository = commentRepository;
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task<PagedResult<GetCommentResponseDto>> Handle(GetCommentsByStockIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting comments for stock {StockId}", request.StockId);

        // Verify stock exists
        var stock = await _stockRepository.GetByIdAsync(request.StockId, cancellationToken);
        if (stock == null)
            throw new StockNotFoundException($"Stock with ID {request.StockId} not found");

        // Get comments
        var comments = await _commentRepository.GetByStockIdAsync(
            request.StockId,
            request.PageNumber,
            request.PageSize,
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
            ? await _commentRepository.GetCountByStockIdAsync(request.StockId, cancellationToken)
            : commentDtos.Count;
        var pagination = PaginationHelper.Build(totalCount, request.PageNumber, request.PageSize);

        _logger.LogInformation("Retrieved {CommentCount} comments for stock {StockId}", commentDtos.Count, request.StockId);

        return new PagedResult<GetCommentResponseDto>
        {
            Items = commentDtos,
            Pagination = pagination
        };
    }
}
