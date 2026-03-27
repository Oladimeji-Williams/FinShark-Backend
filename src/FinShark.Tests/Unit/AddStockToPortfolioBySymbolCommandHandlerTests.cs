using FinShark.Application.Stocks.Commands.AddStockToPortfolio;
using FinShark.Application.Common;
using FinShark.Domain.Entities;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Repositories;
using Moq;
using Xunit;

namespace FinShark.Tests.Unit;

public class AddStockToPortfolioBySymbolCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingLocalStock_AddsToPortfolio()
    {
        // Arrange
        var userId = "user-1";
        var symbol = "TST";
        var stock = new Stock(symbol, "Test Company", 10m) { Id = 1 };
        var stockRepositoryMock = new Mock<IStockRepository>();
        var portfolioRepositoryMock = new Mock<IPortfolioRepository>();
        var fmpServiceMock = new Mock<IFmpService>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AddStockToPortfolioBySymbolCommandHandler>>();

        stockRepositoryMock.Setup(r => r.GetBySymbolAsync(symbol, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
        portfolioRepositoryMock.Setup(r => r.AddStockToPortfolioAsync(userId, stock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new AddStockToPortfolioBySymbolCommandHandler(
            stockRepositoryMock.Object,
            portfolioRepositoryMock.Object,
            fmpServiceMock.Object,
            loggerMock.Object);

        // Act
        var result = await handler.Handle(new AddStockToPortfolioBySymbolCommand(userId, symbol), CancellationToken.None);

        // Assert
        Assert.True(result);
        stockRepositoryMock.Verify(r => r.GetBySymbolAsync(symbol, It.IsAny<CancellationToken>()), Times.Once);
        fmpServiceMock.Verify(s => s.GetStockQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        portfolioRepositoryMock.Verify(r => r.AddStockToPortfolioAsync(userId, stock.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockMissingLocally_FetchesFromFmpAndAddsToPortfolio()
    {
        // Arrange
        var userId = "user-1";
        var symbol = "TST";
        var fmpStock = new Stock(symbol, "Test Company FMP", 15m) { Id = 2 };

        var stockRepositoryMock = new Mock<IStockRepository>();
        var portfolioRepositoryMock = new Mock<IPortfolioRepository>();
        var fmpServiceMock = new Mock<IFmpService>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AddStockToPortfolioBySymbolCommandHandler>>();

        stockRepositoryMock.SetupSequence(r => r.GetBySymbolAsync(symbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null)
            .ReturnsAsync((Stock?)null)
            .ReturnsAsync(fmpStock);

        fmpServiceMock.Setup(s => s.GetStockQuoteAsync(symbol, It.IsAny<CancellationToken>())).ReturnsAsync(fmpStock);
        stockRepositoryMock.Setup(r => r.AddAsync(fmpStock, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        portfolioRepositoryMock.Setup(r => r.AddStockToPortfolioAsync(userId, fmpStock.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new AddStockToPortfolioBySymbolCommandHandler(
            stockRepositoryMock.Object,
            portfolioRepositoryMock.Object,
            fmpServiceMock.Object,
            loggerMock.Object);

        // Act
        var result = await handler.Handle(new AddStockToPortfolioBySymbolCommand(userId, symbol), CancellationToken.None);

        // Assert
        Assert.True(result);
        fmpServiceMock.Verify(s => s.GetStockQuoteAsync(symbol, It.IsAny<CancellationToken>()), Times.Once);
        stockRepositoryMock.Verify(r => r.AddAsync(fmpStock, It.IsAny<CancellationToken>()), Times.Once);
        portfolioRepositoryMock.Verify(r => r.AddStockToPortfolioAsync(userId, fmpStock.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockNotFoundAnywhere_ThrowsStockNotFoundException()
    {
        // Arrange
        var userId = "user-1";
        var symbol = "MISSING";
        var stockRepositoryMock = new Mock<IStockRepository>();
        var portfolioRepositoryMock = new Mock<IPortfolioRepository>();
        var fmpServiceMock = new Mock<IFmpService>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AddStockToPortfolioBySymbolCommandHandler>>();

        stockRepositoryMock.Setup(r => r.GetBySymbolAsync(symbol, It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);
        fmpServiceMock.Setup(s => s.GetStockQuoteAsync(symbol, It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);

        var handler = new AddStockToPortfolioBySymbolCommandHandler(
            stockRepositoryMock.Object,
            portfolioRepositoryMock.Object,
            fmpServiceMock.Object,
            loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<StockNotFoundException>(() => handler.Handle(new AddStockToPortfolioBySymbolCommand(userId, symbol), CancellationToken.None));
    }
}
