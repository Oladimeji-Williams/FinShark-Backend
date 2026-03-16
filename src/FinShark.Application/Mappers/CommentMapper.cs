using FinShark.Application.Dtos;
using FinShark.Application.Comments.Commands.UpdateComment;
using FinShark.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Comment entity to/from DTOs
/// </summary>
public sealed class CommentMapper
{
    /// <summary>
    /// Maps Comment entity to GetCommentResponseDto
    /// </summary>
    public static GetCommentResponseDto ToDto(Comment comment)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));

        return new GetCommentResponseDto
        {
            Id = comment.Id,
            StockId = comment.StockId,
            Title = comment.Title,
            Content = comment.Content,
            Rating = comment.Rating,
            Created = default,
            Modified = default
        };
    }

    /// <summary>
    /// Maps collection of Comment entities to DTOs
    /// </summary>
    public static IEnumerable<GetCommentResponseDto> ToDtoList(IEnumerable<Comment> comments)
    {
        if (comments == null) throw new ArgumentNullException(nameof(comments));
        return comments.Select(ToDto);
    }

    /// <summary>
    /// Maps CreateCommentRequestDto to Comment entity
    /// </summary>
    public static Comment ToEntity(string userId, int stockId, CreateCommentRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return new Comment(
            userId,
            stockId,
            request.Title,
            request.Content,
            request.Rating
        );
    }

    /// <summary>
    /// Updates an existing Comment entity from UpdateCommentRequestDto
    /// </summary>
    public static void UpdateEntity(Comment comment, UpdateCommentRequestDto request)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        if (request == null) throw new ArgumentNullException(nameof(request));

        comment.Update(
            title: request.Title,
            content: request.Content,
            rating: request.Rating
        );
    }

    /// <summary>
    /// Updates an existing Comment entity from UpdateCommentCommand
    /// </summary>
    public static void UpdateEntity(Comment comment, UpdateCommentCommand command)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        if (command == null) throw new ArgumentNullException(nameof(command));

        comment.Update(
            title: command.Title,
            content: command.Content,
            rating: command.Rating
        );
    }
}
