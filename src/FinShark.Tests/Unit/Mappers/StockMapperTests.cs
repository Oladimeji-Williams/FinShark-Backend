using FluentAssertions;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Domain.Entities;
using FinShark.Domain.ValueObjects;
using Xunit;

namespace FinShark.Tests.Unit.Mappers;

public class StockMapperTests
{
    #region ToDto Tests

    [Fact]
    public void ToDto_WithValidStock_ShouldMapCorrectly()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m)
        {
            Sector = SectorType.Technology,
            MarketCap = 2800000000000
        };
        stock.GetType().GetProperty("Id")?.SetValue(stock, 1);
        stock.Comments.Add(new Comment("test-user", stock.Id, "Great stock", "Strong performer", Rating.From(5)));
        stock.Comments.Add(new Comment("test-user", stock.Id, "Solid growth", "Consistent earnings", Rating.From(4)));
        stock.Comments.Add(new Comment("test-user", stock.Id, "Watchlist", "Monitor next quarter", Rating.From(3)));

        // Act
        var result = StockMapper.ToDto(stock);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Symbol.Should().Be("AAPL");
        result.CompanyName.Should().Be("Apple Inc.");
        result.CurrentPrice.Should().Be(150.50m);
        result.Sector.Should().Be(SectorType.Technology);
        result.MarketCap.Should().Be(2800000000000);
        result.Comments.Should().HaveCount(3);
        result.Comments[0].Title.Should().Be("Great stock");
        result.Comments[1].Title.Should().Be("Solid growth");
        result.Comments[2].Title.Should().Be("Watchlist");
    }

    [Fact]
    public void ToDto_WithNullStock_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => StockMapper.ToDto(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToDtoList Tests

    [Fact]
    public void ToDtoList_WithValidStocks_ShouldMapAllCorrectly()
    {
        // Arrange
        var stocks = new List<Stock>
        {
            new Stock("AAPL", "Apple Inc.", 150.50m) { Sector = SectorType.Technology },
            new Stock("MSFT", "Microsoft Corp.", 380.00m) { Sector = SectorType.Technology }
        };

        // Act
        var result = StockMapper.ToDtoList(stocks).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Symbol.Should().Be("AAPL");
        result[1].Symbol.Should().Be("MSFT");
    }

    [Fact]
    public void ToDtoList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => StockMapper.ToDtoList(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDtoList_WithEmptyCollection_ShouldReturnEmpty()
    {
        // Arrange
        var stocks = new List<Stock>();

        // Act
        var result = StockMapper.ToDtoList(stocks).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region ToEntity from DTO Tests

    [Fact]
    public void ToEntity_FromCreateStockRequestDto_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new CreateStockRequestDto
        {
            Symbol = "AAPL",
            CompanyName = "Apple Inc.",
            CurrentPrice = 150.50m,
            Sector = SectorType.Technology,
            MarketCap = 2800000000000
        };

        // Act
        var result = StockMapper.ToEntity(dto);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("AAPL");
        result.CompanyName.Should().Be("Apple Inc.");
        result.CurrentPrice.Should().Be(150.50m);
        result.Sector.Should().Be(SectorType.Technology);
        result.MarketCap.Should().Be(2800000000000);
    }

    [Fact]
    public void ToEntity_FromNullDto_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => StockMapper.ToEntity((CreateStockRequestDto)null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToEntity from Command Tests

    [Fact]
    public void ToEntity_FromCreateStockCommand_ShouldMapCorrectly()
    {
        // Arrange
        var command = new CreateStockCommand(
            "MSFT",
            "Microsoft Corp.",
            380.00m,
            SectorType.Technology,
            10000000000000
        );

        // Act
        var result = StockMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("MSFT");
        result.CompanyName.Should().Be("Microsoft Corp.");
        result.CurrentPrice.Should().Be(380.00m);
        result.Sector.Should().Be(SectorType.Technology);
        result.MarketCap.Should().Be(10000000000000);
    }

    [Fact]
    public void ToEntity_FromNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => StockMapper.ToEntity((CreateStockCommand)null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
