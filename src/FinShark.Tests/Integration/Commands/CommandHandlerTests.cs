using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Queries.GetStocks;
using FinShark.Application.Mappers;
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using FinShark.Persistence;
using FinShark.Persistence.Repositories;
using FinShark.Domain.ValueObjects;
using Xunit;
using MediatR;

namespace FinShark.Tests.Integration.Commands;

public class CreateStockCommandHandlerTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IStockRepository _repository = null!;
    private CreateStockCommandHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_db_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new StockRepository(_context, new MockLogger<StockRepository>());
        _handler = new CreateStockCommandHandler(_repository, new MockLogger<CreateStockCommandHandler>());
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    #region CreateStockCommandHandler Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateStock()
    {
        // Arrange
        var command = new CreateStockCommand(
            "AAPL",
            "Apple Inc.",
            150.50m,
            IndustryType.Technology,
            2800000000000
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var created = await _repository.GetBySymbolAsync("AAPL");
        created.Should().NotBeNull();
        created!.CompanyName.Should().Be("Apple Inc.");
    }

    [Fact]
    public async Task Handle_WithDuplicateSymbol_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stock = new Stock("MSFT", "Microsoft Corp.", 380.00m);
        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        var command = new CreateStockCommand(
            "MSFT",
            "Different Company",
            100.00m
        );

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPreserveEnumValue()
    {
        // Arrange
        var command = new CreateStockCommand(
            "GOOGL",
            "Google Inc.",
            140.00m,
            IndustryType.Technology
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _repository.GetBySymbolAsync("GOOGL");
        created.Should().NotBeNull();
        created!.Industry.Should().Be(IndustryType.Technology);
    }

    #endregion
}

public class GetStocksQueryHandlerTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private IStockRepository _repository = null!;
    private GetStocksQueryHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_db_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new StockRepository(_context, new MockLogger<StockRepository>());
        _handler = new GetStocksQueryHandler(_repository, new MockLogger<GetStocksQueryHandler>());

        // Seed test data
        _context.Stocks.AddRange(
            new Stock("AAPL", "Apple Inc.", 150.50m) { Industry = IndustryType.Technology },
            new Stock("MSFT", "Microsoft Corp.", 380.00m) { Industry = IndustryType.Technology },
            new Stock("JPM", "JP Morgan", 180.00m) { Industry = IndustryType.Finance }
        );
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStocks()
    {
        // Arrange
        var query = new GetStocksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldMaptoDtosCorrectly()
    {
        // Arrange
        var query = new GetStocksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var dtoList = result.ToList();

        // Assert
        dtoList[0].Symbol.Should().Be("AAPL");
        dtoList[0].Industry.Should().Be(IndustryType.Technology);
        dtoList[1].Symbol.Should().Be("MSFT");
        dtoList[2].Industry.Should().Be(IndustryType.Finance);
    }
}

public class MockLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId,
        TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
