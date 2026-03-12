namespace FinShark.Domain.Exceptions;

/// <summary>
/// Base exception for all FinShark domain exceptions
/// </summary>
public abstract class FinSharkException : Exception
{
    public virtual int ErrorCode { get; protected set; } = 1000;

    protected FinSharkException(string message)
        : base(message) { }

    protected FinSharkException(string message, Exception innerException)
        : base(message, innerException) { }
}
