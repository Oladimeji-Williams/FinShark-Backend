namespace FinShark.Domain.Exceptions;

/// <summary>
/// Exception thrown when an external FMP service call fails.
/// </summary>
public sealed class FMPServiceException : FinSharkException
{
    public int? FmpStatusCode { get; }
    public string? Suggestion { get; }

    public FMPServiceException(string message, int? fmpStatusCode = null, string? suggestion = null)
        : base(message)
    {
        ErrorCode = 1002;
        FmpStatusCode = fmpStatusCode;
        Suggestion = suggestion;
    }

    public FMPServiceException(string message, Exception innerException, int? fmpStatusCode = null, string? suggestion = null)
        : base(message, innerException)
    {
        ErrorCode = 1002;
        FmpStatusCode = fmpStatusCode;
        Suggestion = suggestion;
    }
}
