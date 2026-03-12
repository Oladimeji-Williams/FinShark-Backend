namespace FinShark.Application.Dtos;

using FinShark.Domain.ValueObjects;

/// <summary>
/// Data Transfer Object for Comment entity (Read/Get operations)
/// </summary>
public sealed class GetCommentResponseDto
{
    /// <summary>
    /// Unique identifier for the comment
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Foreign key reference to the Stock
    /// </summary>
    public required int StockId { get; init; }

    /// <summary>
    /// Comment title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Comment content/body
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Rating from 1 to 5 stars
    /// </summary>
    public required Rating Rating { get; init; }

    /// <summary>
    /// Timestamp when comment was created
    /// </summary>
    public required DateTime Created { get; init; }

    /// <summary>
    /// Timestamp when comment was last modified
    /// </summary>
    public DateTime? Modified { get; init; }
}

/// <summary>
/// Request DTO for creating a new Comment
/// </summary>
public sealed class CreateCommentRequestDto
{
    /// <summary>
    /// Stock ID that this comment is associated with
    /// </summary>
    public required int StockId { get; init; }

    /// <summary>
    /// Comment title (3-200 characters)
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Comment content (10-5000 characters)
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Rating from 1 to 5 stars
    /// </summary>
    public required Rating Rating { get; init; }
}

/// <summary>
/// Request DTO for updating an existing comment - supports partial updates with optional fields
/// </summary>
public sealed class UpdateCommentRequestDto
{
    /// <summary>
    /// Comment title (optional for partial updates)
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Comment content/body (optional for partial updates)
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Rating from 1 to 5 stars (optional for partial updates)
    /// </summary>
    public Rating? Rating { get; init; }
}
