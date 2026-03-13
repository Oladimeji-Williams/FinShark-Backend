using FinShark.Application.Dtos;

namespace FinShark.Application.Common;

/// <summary>
/// Helper for building pagination metadata.
/// </summary>
public static class PaginationHelper
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static PaginationMetadata Build(int totalCount, int? pageNumber, int? pageSize)
    {
        var isPaged = pageNumber.HasValue || pageSize.HasValue;

        int resolvedPageNumber;
        int resolvedPageSize;

        if (isPaged)
        {
            resolvedPageNumber = Math.Max(1, pageNumber.GetValueOrDefault(1));
            resolvedPageSize = Math.Clamp(pageSize.GetValueOrDefault(DefaultPageSize), 1, MaxPageSize);
        }
        else
        {
            resolvedPageNumber = 1;
            resolvedPageSize = totalCount;
        }

        var totalPages = resolvedPageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)resolvedPageSize);

        return new PaginationMetadata
        {
            TotalCount = totalCount,
            PageNumber = resolvedPageNumber,
            PageSize = resolvedPageSize,
            TotalPages = totalPages,
            HasNextPage = resolvedPageNumber < totalPages,
            HasPreviousPage = resolvedPageNumber > 1 && totalPages > 0
        };
    }
}
