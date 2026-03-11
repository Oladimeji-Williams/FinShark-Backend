using MediatR;

namespace FinShark.Application.Stocks.Commands.CreateStock;

/// <summary>
/// Command to create a new stock
/// Implements CQRS pattern - responsible for state change
/// </summary>
public sealed record CreateStockCommand(
    string Symbol,
    string CompanyName,
    decimal CurrentPrice,
    string Industry = "",
    decimal MarketCap = 0
) : IRequest<int>;