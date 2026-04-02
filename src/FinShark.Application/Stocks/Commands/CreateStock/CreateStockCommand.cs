using MediatorFlow.Core.Contracts;
using FinShark.Application.Dtos;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Stocks.Commands.CreateStock;

/// <summary>
/// Command to create a new stock
/// Implements CQRS pattern - responsible for state change
/// </summary>
public sealed record CreateStockCommand(
    string Symbol,
    string CompanyName,
    decimal CurrentPrice,
    SectorType Sector = default,
    decimal MarketCap = 0
) : IRequest<CreateStockResponseDto>;
