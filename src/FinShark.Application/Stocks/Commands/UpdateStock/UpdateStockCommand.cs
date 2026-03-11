using MediatR;

namespace FinShark.Application.Stocks.Commands.UpdateStock;

/// <summary>
/// Command to update an existing stock
/// </summary>
public sealed record UpdateStockCommand(
    int Id,
    string Symbol,
    string CompanyName,
    decimal CurrentPrice,
    string Industry,
    decimal MarketCap) : IRequest<bool>;
