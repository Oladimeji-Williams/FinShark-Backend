using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Commands.UpdateStock;
using FinShark.Domain.Entities;

namespace FinShark.Application.Mappers;

/// <summary>
/// Manual mapper for Stock entity to/from DTOs
/// Maintains explicit control over mapping logic and improves testability
/// </summary>
public static class StockMapper
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
            Sector = stock.Sector,
            MarketCap = stock.MarketCap,
            Comments = CommentMapper.ToDtoList(stock.Comments).ToList()
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
    /// Maps a stock creation request DTO to a stock entity.
    /// </summary>
    public static Stock ToEntity(CreateStockRequestDto createStockRequestDto)
    {
        ArgumentNullException.ThrowIfNull(createStockRequestDto, nameof(createStockRequestDto));

        return new Stock(
            symbol: createStockRequestDto.Symbol,
            companyName: createStockRequestDto.CompanyName,
            currentPrice: createStockRequestDto.CurrentPrice)
        {
            Sector = createStockRequestDto.Sector,
            MarketCap = createStockRequestDto.MarketCap
        };
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
            Sector = createStockCommand.Sector,
            MarketCap = createStockCommand.MarketCap
        };
    }

    /// <summary>
    /// Updates an existing stock entity from an update command.
    /// </summary>
    public static void UpdateEntity(Stock stock, UpdateStockCommand updateStockCommand)
    {
        ArgumentNullException.ThrowIfNull(stock, nameof(stock));
        ArgumentNullException.ThrowIfNull(updateStockCommand, nameof(updateStockCommand));

        stock.Update(
            symbol: updateStockCommand.Symbol,
            companyName: updateStockCommand.CompanyName,
            currentPrice: updateStockCommand.CurrentPrice,
            sector: updateStockCommand.Sector,
            marketCap: updateStockCommand.MarketCap);
    }
}
