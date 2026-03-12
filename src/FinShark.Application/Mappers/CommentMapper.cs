using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

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
    /// Maps CreateCommentRequestDto to Comment entity
    /// </summary>
    public static Comment ToEntity(int stockId, CreateCommentRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return new Comment(
            stockId,
            request.Title,
            request.Content,
            request.Rating
        );
    }
}
