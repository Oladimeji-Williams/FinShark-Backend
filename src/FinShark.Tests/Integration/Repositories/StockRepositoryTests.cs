using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using FinShark.Persistence;
using FinShark.Persistence.Repositories;
using FinShark.Domain.ValueObjects;
using Xunit;

namespace FinShark.Tests.Integration.Repositories;

public class StockRepositoryTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IStockRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_db_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);

        // Initialize required DbSets
        await _context.Database.EnsureCreatedAsync();
        _repository = new StockRepository(_context, new MockLogger<StockRepository>());
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnStock()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(stock.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetBySymbolAsync Tests

    [Fact]
    public async Task GetBySymbolAsync_WithExistingSymbol_ShouldReturnStock()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySymbolAsync("AAPL");

        // Assert
        result.Should().NotBeNull();
        result!.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetBySymbolAsync_WithDuplicateSymbol_ShouldDetectDuplicate()
    {
        // Arrange
        var stock1 = new Stock("MSFT", "Microsoft Corp.", 380.00m);
        _context.Stocks.Add(stock1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySymbolAsync("MSFT");

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Microsoft Corp.");
    }

    [Fact]
    public async Task GetBySymbolAsync_WithNonExistingSymbol_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySymbolAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleStocks_ShouldReturnAll()
    {
        // Arrange
        _context.Stocks.AddRange(
            new Stock("AAPL", "Apple Inc.", 150.50m),
            new Stock("MSFT", "Microsoft Corp.", 380.00m),
            new Stock("GOOGL", "Google Inc.", 140.00m)
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithNoStocks_ShouldReturnEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithNewStock_ShouldPersist()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m) 
        { 
            Industry = IndustryType.Technology 
        };

        // Act
        await _repository.AddAsync(stock);
        var retrieved = await _repository.GetBySymbolAsync("AAPL");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Symbol.Should().Be("AAPL");
        retrieved.Industry.Should().Be(IndustryType.Technology);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedStock_ShouldPersist()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        // Act
        stock.Update(currentPrice: 175.00m);
        await _repository.UpdateAsync(stock);
        var retrieved = await _repository.GetByIdAsync(stock.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.CurrentPrice.Should().Be(175.00m);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingStock_ShouldRemove()
    {
        // Arrange
        var stock = new Stock("AAPL", "Apple Inc.", 150.50m);
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();
        var stockId = stock.Id;

        // Act
        await _repository.DeleteAsync(stock);
        var retrieved = await _repository.GetByIdAsync(stockId);

        // Assert
        retrieved.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Mock logger for testing - prevents logging errors during unit tests
/// </summary>
public class MockLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, 
        TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
