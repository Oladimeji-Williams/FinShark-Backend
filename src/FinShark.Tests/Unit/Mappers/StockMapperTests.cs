using FluentAssertions;
using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Domain.Entities;
using FinShark.Domain.Enums;
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
            Industry = IndustryType.Technology,
            MarketCap = 2800000000000
        };
        stock.GetType().GetProperty("Id")?.SetValue(stock, 1);

        // Act
        var result = StockMapper.ToDto(stock);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Symbol.Should().Be("AAPL");
        result.CompanyName.Should().Be("Apple Inc.");
        result.CurrentPrice.Should().Be(150.50m);
        result.Industry.Should().Be(IndustryType.Technology);
        result.MarketCap.Should().Be(2800000000000);
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
            new Stock("AAPL", "Apple Inc.", 150.50m) { Industry = IndustryType.Technology },
            new Stock("MSFT", "Microsoft Corp.", 380.00m) { Industry = IndustryType.Technology }
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
            Industry = IndustryType.Technology,
            MarketCap = 2800000000000
        };

        // Act
        var result = StockMapper.ToEntity(dto);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("AAPL");
        result.CompanyName.Should().Be("Apple Inc.");
        result.CurrentPrice.Should().Be(150.50m);
        result.Industry.Should().Be(IndustryType.Technology);
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
            IndustryType.Technology,
            10000000000000
        );

        // Act
        var result = StockMapper.ToEntity(command);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("MSFT");
        result.CompanyName.Should().Be("Microsoft Corp.");
        result.CurrentPrice.Should().Be(380.00m);
        result.Industry.Should().Be(IndustryType.Technology);
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
