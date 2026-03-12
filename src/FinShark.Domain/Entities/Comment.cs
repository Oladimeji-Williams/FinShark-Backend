namespace FinShark.Domain.Entities;

using FinShark.Domain.ValueObjects;

/// <summary>
/// Represents a comment on a stock
/// </summary>
public sealed class Comment : BaseEntity
{
    public int StockId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Rating Rating { get; private set; }
    
    // Navigation property
    public Stock Stock { get; set; } = null!;

    private Comment() { } // EF Core

    public Comment(int stockId, string title, string content, Rating rating)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));
        if (!rating.IsValid)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        StockId = stockId;
        Title = title;
        Content = content;
        Rating = rating;
        Created = DateTime.UtcNow;
    }

    /// <summary>
    /// Update comment properties
    /// </summary>
    public void Update(string? title = null, string? content = null, Rating? rating = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title;
        if (!string.IsNullOrWhiteSpace(content))
            Content = content;
        if (rating.HasValue)
        {
            if (!rating.Value.IsValid)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
            Rating = rating.Value;
        }
        
        Modified = DateTime.UtcNow;
    }
}
