using FinShark.Application.Dtos;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Stock entity to/from DTOs
/// Maintains explicit control over mapping logic and improves testability
/// </summary>
public sealed class StockMapper
{
    /// <summary>
    /// Maps Stock entity to StockDto (read operation)
    /// </summary>
    public static StockDto ToDto(Stock stock)
    {
        ArgumentNullException.ThrowIfNull(stock, nameof(stock));

        return new StockDto
        {
            Id = stock.Id,
            Symbol = stock.Symbol,
            CompanyName = stock.CompanyName,
            CurrentPrice = stock.CurrentPrice,
            Industry = stock.Industry,
            MarketCap = stock.MarketCap,
            Created = stock.Created,
            Modified = stock.Modified
        };
    }

    /// <summary>
    /// Maps collection of Stock entities to StockDtos
    /// </summary>
    public static IEnumerable<StockDto> ToDtoList(IEnumerable<Stock> stocks)
    {
        ArgumentNullException.ThrowIfNull(stocks, nameof(stocks));
        return stocks.Select(ToDto);
    }

    /// <summary>
    /// Maps CreateStockRequestDto to Stock entity
    /// </summary>
    public static Stock ToEntity(CreateStockRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return new Stock(
            symbol: request.Symbol,
            companyName: request.CompanyName,
            currentPrice: request.CurrentPrice
        )
        {
            Industry = request.Industry,
            MarketCap = request.MarketCap
        };
    }

    /// <summary>
    /// Updates an existing Stock entity from CreateStockRequestDto
    /// </summary>
    public static void UpdateEntity(Stock stock, CreateStockRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(stock, nameof(stock));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        stock.Update(
            symbol: request.Symbol,
            companyName: request.CompanyName,
            currentPrice: request.CurrentPrice,
            industry: request.Industry,
            marketCap: request.MarketCap
        );
    }
}
