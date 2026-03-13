namespace FinShark.Application.Dtos;

/// <summary>
/// Paged result wrapper with pagination metadata.
/// </summary>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required PaginationMetadata Pagination { get; init; }
}

/// <summary>
/// Pagination metadata for list responses.
/// </summary>
public sealed record PaginationMetadata
{
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
}
