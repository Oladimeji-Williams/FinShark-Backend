using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Stock entity to/from DTOs
/// Maintains explicit control over mapping logic and improves testability
/// </summary>
public sealed class StockMapper
{
    /// <summary>
    /// Maps Stock entity to GetStockResponseDto (read operation)
    /// </summary>
    public static GetStockResponseDto ToDto(Stock stock)
    {
        ArgumentNullException.ThrowIfNull(stock, nameof(stock));

        return new GetStockResponseDto
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
    /// Maps collection of Stock entities to GetStockResponseDtos
    /// </summary>
    public static IEnumerable<GetStockResponseDto> ToDtoList(IEnumerable<Stock> stocks)
    {
        ArgumentNullException.ThrowIfNull(stocks, nameof(stocks));
        return stocks.Select(ToDto);
    }

    /// <summary>
    /// Maps CreateStockRequestDto to Stock entity
    /// </summary>
    public static Stock ToEntity(CreateStockRequestDto createStockRequestDto)
    {
        ArgumentNullException.ThrowIfNull(createStockRequestDto, nameof(createStockRequestDto));

        return new Stock(
            symbol: createStockRequestDto.Symbol,
            companyName: createStockRequestDto.CompanyName,
            currentPrice: createStockRequestDto.CurrentPrice
        )
        {
            Industry = createStockRequestDto.Industry,
            MarketCap = createStockRequestDto.MarketCap
        };
    }

    /// <summary>
    /// Updates an existing Stock entity from CreateStockRequestDto
    /// </summary>
    public static void UpdateEntity(Stock stock, CreateStockRequestDto createStockRequestDto)
    {
        ArgumentNullException.ThrowIfNull(stock, nameof(stock));
        ArgumentNullException.ThrowIfNull(createStockRequestDto, nameof(createStockRequestDto));

        stock.Update(
            symbol: createStockRequestDto.Symbol,
            companyName: createStockRequestDto.CompanyName,
            currentPrice: createStockRequestDto.CurrentPrice,
            industry: createStockRequestDto.Industry,
            marketCap: createStockRequestDto.MarketCap
        );
    }

    /// <summary>
    /// Maps CreateStockCommand directly to Stock entity (eliminates intermediate DTO)
    /// </summary>
    public static Stock ToEntity(CreateStockCommand createStockCommand)
    {
        ArgumentNullException.ThrowIfNull(createStockCommand, nameof(createStockCommand));

        return new Stock(
            symbol: createStockCommand.Symbol,
            companyName: createStockCommand.CompanyName,
            currentPrice: createStockCommand.CurrentPrice
        )
        {
            Industry = createStockCommand.Industry,
            MarketCap = createStockCommand.MarketCap
        };
    }
}
