namespace FinShark.Domain.Exceptions;

/// <summary>
/// Exception thrown when a stock with the same symbol already exists
/// </summary>
public sealed class StockAlreadyExistsException : FinSharkException
{
    public StockAlreadyExistsException(string message)
        : base(message)
    {
        ErrorCode = 1002;
    }
}
