using MediatorFlow.Core.Contracts;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Stocks.Commands.UpdateStock;

/// <summary>
/// Command to update an existing stock - supports partial updates with optional fields
/// </summary>
public sealed record UpdateStockCommand(
    int Id,
    string? Symbol = null,
    string? CompanyName = null,
    decimal? CurrentPrice = null,
    SectorType? Sector = null,
    decimal? MarketCap = null) : IRequest<bool>;
