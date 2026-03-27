using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Queries.GetStocks;
using FinShark.Application.Mappers;
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using FinShark.Persistence;
using FinShark.Persistence.Repositories;
using FinShark.Domain.ValueObjects;
using FinShark.Domain.Queries;
using FinShark.Domain.Exceptions;
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
            SectorType.Technology,
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
    public async Task Handle_WithDuplicateSymbol_ShouldThrowStockAlreadyExistsException()
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
            .ThrowAsync<StockAlreadyExistsException>()
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
            SectorType.Technology
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var created = await _repository.GetBySymbolAsync("GOOGL");
        created.Should().NotBeNull();
        created!.Sector.Should().Be(SectorType.Technology);
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
            new Stock("AAPL", "Apple Inc.", 150.50m) { Sector = SectorType.Technology },
            new Stock("MSFT", "Microsoft Corp.", 380.00m) { Sector = SectorType.Technology },
            new Stock("JPM", "JP Morgan", 180.00m) { Sector = SectorType.Finance }
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
        var query = new GetStocksQuery(new StockQueryRequestDto());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.Pagination.TotalCount.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        result.Pagination.PageSize.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(1);
        result.Pagination.HasNextPage.Should().BeFalse();
        result.Pagination.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldMaptoDtosCorrectly()
    {
        // Arrange
        var query = new GetStocksQuery(new StockQueryRequestDto());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var dtoList = result.Items.ToList();

        // Assert
        dtoList[0].Symbol.Should().Be("AAPL");
        dtoList[0].Sector.Should().Be(SectorType.Technology);
        dtoList[1].Symbol.Should().Be("JPM");
        dtoList[1].Sector.Should().Be(SectorType.Finance);
        dtoList[2].Symbol.Should().Be("MSFT");
        dtoList[2].Sector.Should().Be(SectorType.Technology);
    }

    [Fact]
    public async Task Handle_WithSectorFilter_ShouldReturnOnlyMatching()
    {
        // Arrange
        var parameters = new StockQueryRequestDto
        {
            Sector = SectorType.Technology
        };
        var query = new GetStocksQuery(parameters);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.All(s => s.Sector == SectorType.Technology).Should().BeTrue();
        result.Pagination.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSortingAndPagination_ShouldReturnExpectedPage()
    {
        // Arrange
        var parameters = new StockQueryRequestDto
        {
            SortBy = StockSortBy.CurrentPrice,
            SortDirection = SortDirection.Desc,
            PageNumber = 1,
            PageSize = 2
        };
        var query = new GetStocksQuery(parameters);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Symbol.Should().Be("MSFT");
        result.Items[1].Symbol.Should().Be("JPM");
        result.Pagination.TotalCount.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        result.Pagination.PageSize.Should().Be(2);
        result.Pagination.TotalPages.Should().Be(2);
    }
}
public class MockLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId,
        TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
