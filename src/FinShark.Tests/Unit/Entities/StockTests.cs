using FluentAssertions;
using FinShark.Domain.Entities;
using FinShark.Domain.Enums;
using Xunit;

namespace FinShark.Tests.Unit.Entities;

public class StockTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidData_ShouldCreateStock()
    {
        // Act
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Assert
        stock.Symbol.Should().Be("AAPL");
        stock.CompanyName.Should().Be("Apple Inc.");
        stock.CurrentPrice.Should().Be(150.50m);
        stock.Purchase.Should().Be(0);
        stock.LastDiv.Should().Be(0);
        stock.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithPurchaseAndDividend_ShouldInitializeCorrectly()
    {
        // Act
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m, 120.00m, 0.88m);

        // Assert
        stock.Purchase.Should().Be(120.00m);
        stock.LastDiv.Should().Be(0.88m);
    }

    [Fact]
    public void Constructor_WithEmptySymbol_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Stock("", "Apple Inc.", 150.50m);
        action.Should().Throw<ArgumentException>().WithMessage("*Symbol*");
    }

    [Fact]
    public void Constructor_WithNullSymbol_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Stock(null!, "Apple Inc.", 150.50m);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyCompanyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Stock("AAPL", "", 150.50m);
        action.Should().Throw<ArgumentException>().WithMessage("*Company*");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Stock("AAPL", "Apple Inc.", 0);
        action.Should().Throw<ArgumentException>().WithMessage("*price*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new Stock("AAPL", "Apple Inc.", -100);
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Update Method Tests

    [Fact]
    public void Update_WithSymbol_ShouldUpdateSymbol()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(symbol: "APPL");

        // Assert
        stock.Symbol.Should().Be("APPL");
        stock.CompanyName.Should().Be("Apple Inc."); // Unchanged
    }

    [Fact]
    public void Update_WithCompanyName_ShouldUpdateCompanyName()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(companyName: "Apple Corporation");

        // Assert
        stock.CompanyName.Should().Be("Apple Corporation");
        stock.Symbol.Should().Be("AAPL"); // Unchanged
    }

    [Fact]
    public void Update_WithPrice_ShouldUpdatePrice()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(currentPrice: 175.00m);

        // Assert
        stock.CurrentPrice.Should().Be(175.00m);
    }

    [Fact]
    public void Update_WithIndustry_ShouldUpdateIndustry()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(industry: IndustryType.Technology);

        // Assert
        stock.Industry.Should().Be(IndustryType.Technology);
    }

    [Fact]
    public void Update_WithMarketCap_ShouldUpdateMarketCap()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(marketCap: 2800000000000);

        // Assert
        stock.MarketCap.Should().Be(2800000000000);
    }

    [Fact]
    public void Update_WithMultipleFields_ShouldUpdateAll()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(
            symbol: "APPL",
            companyName: "Apple Corp",
            currentPrice: 175.00m,
            industry: IndustryType.Technology,
            marketCap: 3000000000000
        );

        // Assert
        stock.Symbol.Should().Be("APPL");
        stock.CompanyName.Should().Be("Apple Corp");
        stock.CurrentPrice.Should().Be(175.00m);
        stock.Industry.Should().Be(IndustryType.Technology);
        stock.MarketCap.Should().Be(3000000000000);
    }

    [Fact]
    public void Update_WithNullSymbol_ShouldNotUpdate()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(symbol: null);

        // Assert
        stock.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public void Update_WithZeroPrice_ShouldNotUpdate()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(currentPrice: 0);

        // Assert
        stock.CurrentPrice.Should().Be(150.50m);
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldNotUpdate()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Act
        stock.Update(currentPrice: -100);

        // Assert
        stock.CurrentPrice.Should().Be(150.50m);
    }

    [Fact]
    public void Update_ShouldUpdateModifiedTimestamp()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        var originalModified = stock.Modified;

        // Act - Wait a moment to ensure timestamp changes
        System.Threading.Thread.Sleep(100);
        stock.Update(symbol: "APPL");

        // Assert
        stock.Modified.Should().BeAfter(originalModified ?? DateTime.MinValue);
    }

    #endregion

    #region Comments Collection Tests

    [Fact]
    public void Comments_ShouldInitializeAsEmptyList()
    {
        // Act
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Assert
        stock.Comments.Should().NotBeNull();
        stock.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Comments_ShouldAllowAddingComments()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        var comment = new Comment { Title = "Great stock", Content = "Strong performer", Stock = stock };

        // Act
        stock.Comments.Add(comment);

        // Assert
        stock.Comments.Should().HaveCount(1);
        stock.Comments.First().Title.Should().Be("Great stock");
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void NewStock_ShouldHaveDefaultIndustry()
    {
        // Act
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Assert
        stock.Industry.Should().Be(IndustryType.Other);
    }

    [Fact]
    public void NewStock_ShouldHaveZeroMarketCap()
    {
        // Act
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);

        // Assert
        stock.MarketCap.Should().Be(0);
    }

    #endregion
}
