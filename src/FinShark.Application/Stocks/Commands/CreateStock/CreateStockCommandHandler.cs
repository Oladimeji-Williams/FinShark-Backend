using FinShark.Application.Dtos;
using FinShark.Application.Mappers;
using FinShark.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Stocks.Commands.CreateStock;

/// <summary>
/// Handler for creating a new stock
/// Implements CQRS pattern - processes state-changing operations
/// Includes logging for audit trail and debugging
/// </summary>
public sealed class CreateStockCommandHandler(
    IStockRepository stockRepository,
    ILogger<CreateStockCommandHandler> logger) : IRequestHandler<CreateStockCommand, CreateStockResponseDto>
{
    private readonly IStockRepository _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
    private readonly ILogger<CreateStockCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CreateStockResponseDto> Handle(
        CreateStockCommand request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("Creating new stock with symbol: {Symbol}, company: {CompanyName}", 
            request.Symbol, request.CompanyName);

        try
        {
            // Check if stock with this symbol already exists
            var existingStock = await _stockRepository.GetBySymbolAsync(request.Symbol, cancellationToken);
            if (existingStock != null)
            {
                _logger.LogWarning("Stock with symbol {Symbol} already exists", request.Symbol);
                throw new InvalidOperationException($"A stock with symbol '{request.Symbol}' already exists.");
            }

            // Map command directly to entity (eliminates intermediate DTO)
            var stock = StockMapper.ToEntity(request);

            await _stockRepository.AddAsync(stock);

            _logger.LogInformation("Stock created successfully with ID: {StockId}", stock.Id);

            return new CreateStockResponseDto { Id = stock.Id };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock with symbol: {Symbol}", request.Symbol);
            throw;
        }
    }
}