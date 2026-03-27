namespace FinShark.Domain.Queries;

public sealed class CommentQueryParameters
{
    public int? StockId { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string? StockSymbol { get; init; }
    public int? MinRating { get; init; }
    public int? MaxRating { get; init; }
    public string? TitleContains { get; init; }
    public string? ContentContains { get; init; }
    public CommentSortBy SortBy { get; init; } = CommentSortBy.Created;
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}

public enum CommentSortBy
{
    Created,
    Rating,
    Title,
    Symbol
}
