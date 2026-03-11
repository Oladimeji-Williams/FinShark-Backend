using MediatR;

namespace FinShark.Application.Stocks.Commands.DeleteStock;

/// <summary>
/// Command to delete a stock
/// </summary>
public sealed record DeleteStockCommand(int Id) : IRequest<bool>;
