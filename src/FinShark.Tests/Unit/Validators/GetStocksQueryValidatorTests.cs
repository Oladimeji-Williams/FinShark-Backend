using FluentAssertions;
using FinShark.Application.Stocks.Queries.GetStocks;
using FinShark.Application.Stocks.Validators;
using FinShark.Domain.Queries;
using Xunit;

namespace FinShark.Tests.Unit.Validators;

public class GetStocksQueryValidatorTests
{
    private readonly GetStocksQueryValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WithValidParameters_ShouldPass()
    {
        // Arrange
        var query = new GetStocksQuery(new StockQueryParameters
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = StockSortBy.Symbol,
            SortDirection = SortDirection.Asc
        });

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidPageSize_ShouldFail()
    {
        // Arrange
        var query = new GetStocksQuery(new StockQueryParameters
        {
            PageNumber = 1,
            PageSize = 101
        });

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PageSize"));
    }

    [Fact]
    public async Task ValidateAsync_WithMinPriceGreaterThanMaxPrice_ShouldFail()
    {
        // Arrange
        var query = new GetStocksQuery(new StockQueryParameters
        {
            MinPrice = 200,
            MaxPrice = 100
        });

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidSortDirection_ShouldFail()
    {
        // Arrange
        var query = new GetStocksQuery(new StockQueryParameters
        {
            SortDirection = (SortDirection)99
        });

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
