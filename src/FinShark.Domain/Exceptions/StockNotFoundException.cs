namespace FinShark.Domain.Exceptions;

/// <summary>
/// Exception thrown when a stock is not found
/// </summary>
public sealed class StockNotFoundException : FinSharkException
{
    public StockNotFoundException(string message)
        : base(message)
    {
        ErrorCode = 1001;
    }
}
