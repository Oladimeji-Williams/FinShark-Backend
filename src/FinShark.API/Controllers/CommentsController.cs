using MediatR;
using Microsoft.AspNetCore.Mvc;
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
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IMediator mediator, ILogger<CommentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all comments
    /// </summary>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetCommentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllComments([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        _logger.LogInformation("GetAllComments called");

        var query = new GetAllCommentsQuery(pageNumber, pageSize);
        var comments = await _mediator.Send(query);

        return Ok(new ApiResponse<IEnumerable<GetCommentResponseDto>>
        {
            Success = true,
            Data = comments,
            Message = "Comments retrieved successfully"
        });
    }

    /// <summary>
    /// Get a comment by ID
    /// </summary>
    /// <param name="id">Comment ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetCommentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(int id)
    {
        _logger.LogInformation("GetCommentById called for ID: {CommentId}", id);

        var query = new GetCommentByIdQuery(id);
        var comment = await _mediator.Send(query);

        return Ok(new ApiResponse<GetCommentResponseDto>
        {
            Success = true,
            Data = comment,
            Message = "Comment retrieved successfully"
        });
    }

    /// <summary>
    /// Get comments for a specific stock
    /// </summary>
    /// <param name="stockId">Stock ID</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    [HttpGet("stock/{stockId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetCommentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentsByStockId(int stockId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        _logger.LogInformation("GetCommentsByStockId called for stock ID: {StockId}", stockId);

        var query = new GetCommentsByStockIdQuery(stockId, pageNumber, pageSize);
        var comments = await _mediator.Send(query);

        return Ok(new ApiResponse<IEnumerable<GetCommentResponseDto>>
        {
            Success = true,
            Data = comments,
            Message = "Comments retrieved successfully"
        });
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    /// <param name="request">Comment creation request</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequestDto request)
    {
        _logger.LogInformation("CreateComment called for stock ID: {StockId}", request.StockId);

        var command = new CreateCommentCommand(request.StockId, request.Title, request.Content, request.Rating);
        var commentId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetCommentById), new { id = commentId }, new ApiResponse<int>
        {
            Success = true,
            Data = commentId,
            Message = "Comment created successfully"
        });
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    /// <param name="id">Comment ID</param>
    /// <param name="request">Comment update request</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequestDto request)
    {
        _logger.LogInformation("UpdateComment called for ID: {CommentId}", id);

        var command = new UpdateCommentCommand(id, request.Title, request.Content, request.Rating);
        await _mediator.Send(command);

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Comment updated successfully"
        });
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    /// <param name="id">Comment ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int id)
    {
        _logger.LogInformation("DeleteComment called for ID: {CommentId}", id);

        var command = new DeleteCommentCommand(id);
        await _mediator.Send(command);

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Comment deleted successfully"
        });
    }
}
