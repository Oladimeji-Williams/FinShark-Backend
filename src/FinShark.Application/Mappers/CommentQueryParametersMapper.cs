using FinShark.Application.Dtos;
using FinShark.Domain.Queries;

namespace FinShark.Application.Mappers;

public static class CommentQueryParametersMapper
{
    public static CommentQueryParameters ToDomain(CommentQueryRequestDto request, int? stockId = null)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return new CommentQueryParameters
        {
            StockId = stockId ?? request.StockId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            StockSymbol = request.StockSymbol,
            MinRating = request.MinRating,
            MaxRating = request.MaxRating,
            TitleContains = request.TitleContains,
            ContentContains = request.ContentContains,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };
    }
}
