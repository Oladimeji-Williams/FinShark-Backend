using FinShark.Application.Dtos;
using FinShark.Domain.Queries;

namespace FinShark.Application.Mappers;

public static class StockQueryParametersMapper
{
    public static StockQueryParameters ToDomain(StockQueryRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return new StockQueryParameters
        {
            Symbol = request.Symbol,
            CompanyName = request.CompanyName,
            Sector = request.Sector,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinMarketCap = request.MinMarketCap,
            MaxMarketCap = request.MaxMarketCap,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
