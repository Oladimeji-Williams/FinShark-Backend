using FinShark.Application.Dtos;
using FinShark.Application.Comments.Commands.CreateComment;
using FinShark.Application.Comments.Commands.UpdateComment;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Comment entity to/from DTOs
/// </summary>
public static class CommentMapper
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
            Created = comment.Created,
            Modified = comment.Modified
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
    /// Maps CreateCommentCommand to Comment entity
    /// </summary>
    public static Comment ToEntity(CreateCommentCommand command)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        return new Comment(
            command.UserId,
            command.StockId,
            command.Title,
            command.Content,
            command.Rating
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
