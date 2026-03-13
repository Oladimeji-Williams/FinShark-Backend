using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Application.Comments.Commands.UpdateComment;
using FinShark.Application.Comments.Commands.DeleteComment;
using FinShark.Application.Comments.Queries.GetCommentById;
using FinShark.Application.Comments.Queries.GetCommentsByStockId;
using FinShark.Application.Comments.Queries.GetAllComments;
using FinShark.Application.Dtos;

namespace FinShark.API.Controllers;

/// <summary>
/// Controller for managing stock comments
/// Handles all comment-related HTTP requests
/// </summary>
[ApiController]
[Route("api/comments")]
[Produces("application/json")]
public sealed class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IMediator mediator, ILogger<CommentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all comments
    /// </summary>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetCommentResponseDto>>>> GetAllComments(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/comments - Retrieving comments");

        var query = new GetAllCommentsQuery(pageNumber, pageSize);
        var comments = await _mediator.Send(query, cancellationToken);

        var response = ApiResponse<PagedResult<GetCommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get a comment by ID
    /// </summary>
    /// <param name="id">Comment ID</param>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetCommentResponseDto>>> GetCommentById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/comments/{Id} - Retrieving comment", id);

        var query = new GetCommentByIdQuery(id);
        var comment = await _mediator.Send(query, cancellationToken);

        var response = ApiResponse<GetCommentResponseDto>.SuccessResponse(comment, "Comment retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get comments for a specific stock (nested under stocks)
    /// </summary>
    /// <param name="stockId">Stock ID</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    [HttpGet("/api/stocks/{stockId:int:min(1)}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetCommentResponseDto>>>> GetCommentsByStockId(
        int stockId,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks/{StockId}/comments - Retrieving comments", stockId);

        var query = new GetCommentsByStockIdQuery(stockId, pageNumber, pageSize);
        var comments = await _mediator.Send(query, cancellationToken);

        var response = ApiResponse<PagedResult<GetCommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    /// <param name="request">Comment creation request</param>
    [HttpPost("/api/stocks/{stockId:int:min(1)}/comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> CreateComment(
        int stockId,
        [FromBody] CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/stocks/{StockId}/comments - Creating comment", stockId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("POST /api/stocks/{StockId}/comments - User ID not found in token", stockId);
            return Unauthorized(ApiResponse<int>.FailureResponse("User not authenticated"));
        }

        var command = new CreateCommentCommand(userId, stockId, request.Title, request.Content, request.Rating);
        var commentId = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<int>.SuccessResponse(commentId, "Comment created successfully");
        return CreatedAtAction(nameof(GetCommentById), new { id = commentId }, response);
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <param name="request">Comment update request</param>
    [HttpPatch("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateComment(
        int id,
        [FromBody] UpdateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PATCH /api/comments/{Id} - Updating comment", id);

        var command = new UpdateCommentCommand(id, request.Title, request.Content, request.Rating);
        var result = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<bool>.SuccessResponse(result, "Comment updated successfully");
        return Ok(response);
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="id">Comment ID</param>
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /api/comments/{Id} - Deleting comment", id);

        var command = new DeleteCommentCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<bool>.SuccessResponse(result, "Comment deleted successfully");
        return Ok(response);
    }
}
