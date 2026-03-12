using FluentAssertions;
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Validators;
using FinShark.Domain.Enums;
using Xunit;

namespace FinShark.Tests.Unit.Validators;

public class CreateStockValidatorTests
{
    private readonly CreateStockValidator _validator;

    public CreateStockValidatorTests()
    {
        _validator = new CreateStockValidator();
    }

    #region Symbol Validation

    [Fact]
    public async Task ValidateAsync_WithValidSymbol_ShouldPass()
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "AAPL",
            CompanyName = "Apple Inc.",
            CurrentPrice = 150.50m,
            Industry = IndustryType.Technology
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithEmptySymbol_ShouldFail()
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "",
            CompanyName = "Apple Inc.",
            CurrentPrice = 150.50m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol");
    }

    [Fact]
    public async Task ValidateAsync_WithSymbolExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "VERYLONGSYMBOL", // 14 characters, max is 10
            CompanyName = "Apple Inc.",
            CurrentPrice = 150.50m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("AAPL")]
    [InlineData("MSFT.L")]
    [InlineData("BRK.B")]
    [InlineData("ABC123")]
    public async Task ValidateAsync_WithValidSymbolFormats_ShouldPass(string symbol)
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = symbol,
            CompanyName = "Test Company",
            CurrentPrice = 100m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("aapl")]    // lowercase
    [InlineData("Aapl")]    // mixed case
    [InlineData("AAP-L")]   // dash instead of dot
    public async Task ValidateAsync_WithInvalidSymbolFormat_ShouldFail(string symbol)
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = symbol,
            CompanyName = "Test Company",
            CurrentPrice = 100m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region CompanyName Validation

    [Fact]
    public async Task ValidateAsync_WithEmptyCompanyName_ShouldFail()
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "AAPL",
            CompanyName = "",
            CurrentPrice = 150.50m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithCompanyNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "AAPL",
            CompanyName = new string('A', 256), // 256 characters, max is 255
            CurrentPrice = 150.50m
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region CurrentPrice Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task ValidateAsync_WithInvalidPrice_ShouldFail(decimal price)
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "AAPL",
            CompanyName = "Apple Inc.",
            CurrentPrice = price
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(150.50)]
    [InlineData(10000)]
    public async Task ValidateAsync_WithValidPrice_ShouldPass(decimal price)
    {
        // Arrange
        var dto = new CreateStockRequestDto 
        { 
            Symbol = "AAPL",
            CompanyName = "Apple Inc.",
            CurrentPrice = price
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Complete Valid Request

    [Fact]
    public async Task ValidateAsync_WithCompleteValidRequest_ShouldPass()
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
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
