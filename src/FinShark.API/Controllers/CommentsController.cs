using FinShark.API.Extensions;
using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Application.Comments.Commands.DeleteComment;
using FinShark.Application.Comments.Commands.UpdateComment;
using FinShark.Application.Comments.Queries.GetAllComments;
using FinShark.Application.Comments.Queries.GetCommentById;
using FinShark.Application.Comments.Queries.GetCommentsByStockId;
using FinShark.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

/// <summary>
/// Controller for managing stock comments.
/// </summary>
[Route("api/comments")]
public sealed class CommentsController(IMediator mediator, ILogger<CommentsController> logger) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<CommentsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GetCommentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetCommentResponseDto>>>> GetAllComments(
        [FromQuery] CommentQueryRequestDto queryParameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /api/comments - Retrieving comments");

        var comments = await _mediator.Send(
            new GetAllCommentsQuery(queryParameters ?? new CommentQueryRequestDto()),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<GetCommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
    }

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<GetCommentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetCommentResponseDto>>> GetCommentById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/comments/{Id} - Retrieving comment", id);

        var comment = await _mediator.Send(new GetCommentByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<GetCommentResponseDto>.SuccessResponse(comment, "Comment retrieved successfully"));
    }

    [HttpGet("/api/stocks/{stockId:int:min(1)}/comments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GetCommentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetCommentResponseDto>>>> GetCommentsByStockId(
        int stockId,
        [FromQuery] CommentQueryRequestDto queryParameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /api/stocks/{StockId}/comments - Retrieving comments", stockId);

        var comments = await _mediator.Send(
            new GetCommentsByStockIdQuery(stockId, queryParameters ?? new CommentQueryRequestDto()),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<GetCommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
    }

    [HttpPost("/api/stocks/{stockId:int:min(1)}/comments")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> CreateComment(
        int stockId,
        [FromBody] CreateCommentRequestDto createCommentRequestDto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/stocks/{StockId}/comments - Creating comment", stockId);

        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("POST /api/stocks/{StockId}/comments - User ID not found in token", stockId);
            return Unauthorized(ApiResponse<int>.FailureResponse("User not authenticated"));
        }

        var commentId = await _mediator.Send(
            new CreateCommentCommand(
                userId,
                stockId,
                createCommentRequestDto.Title,
                createCommentRequestDto.Content,
                createCommentRequestDto.Rating),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetCommentById),
            new { id = commentId },
            ApiResponse<int>.SuccessResponse(commentId, "Comment created successfully"));
    }

    [HttpPatch("{id:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateComment(
        int id,
        [FromBody] UpdateCommentRequestDto updateCommentRequestDto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PATCH /api/comments/{Id} - Updating comment", id);

        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("PATCH /api/comments/{Id} - User ID not found in token", id);
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        var result = await _mediator.Send(
            new UpdateCommentCommand(
                id,
                userId,
                User.IsInRole("Admin"),
                updateCommentRequestDto.Title,
                updateCommentRequestDto.Content,
                updateCommentRequestDto.Rating),
            cancellationToken);

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Comment updated successfully"));
    }

    [HttpDelete("{id:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(
        int id,
        CancellationToken cancellationToken,
        [FromQuery] bool hardDelete = false)
    {
        _logger.LogInformation("DELETE /api/comments/{Id} - Deleting comment {CommentId}, hardDelete={HardDelete}", id, id, hardDelete);

        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("DELETE /api/comments/{Id} - User ID not found in token", id);
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        if (hardDelete && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("Hard delete attempt by non-admin for comment {CommentId}", id);
            return Forbid();
        }

        var result = await _mediator.Send(
            new DeleteCommentCommand(id, userId, User.IsInRole("Admin"), HardDelete: hardDelete),
            cancellationToken);

        return Ok(ApiResponse<bool>.SuccessResponse(
            result,
            hardDelete ? "Comment hard deleted successfully" : "Comment soft deleted successfully"));
    }
}
