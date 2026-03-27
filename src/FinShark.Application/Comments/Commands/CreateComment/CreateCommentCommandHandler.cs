using MediatR;
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using FinShark.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Comments.Commands.CreateComment;

/// <summary>
/// Handler for creating a new comment
/// </summary>
public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, int>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<CreateCommentCommandHandler> _logger;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IStockRepository stockRepository,
        ILogger<CreateCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task<int> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating comment for stock {StockId}", request.StockId);

        // Verify stock exists
        var stock = await _stockRepository.GetByIdAsync(request.StockId, cancellationToken);
        if (stock == null)
            throw new StockNotFoundException($"Stock with ID {request.StockId} not found");

        // Create comment entity
        var comment = FinShark.Application.Mappers.CommentMapper.ToEntity(request);

        // Add to repository
        await _commentRepository.AddAsync(comment, cancellationToken);

        _logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);

        return comment.Id;
    }
}
