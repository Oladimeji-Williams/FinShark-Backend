namespace FinShark.Application.Dtos;

/// <summary>
/// Generic API response wrapper for consistent response formatting
/// </summary>
public sealed record ApiResponse<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public int? FmpStatusCode { get; init; }
    public string? Suggestion { get; init; }
    public IEnumerable<string>? Errors { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> FailureResponse(string message, int? fmpStatusCode = null, string? suggestion = null, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            FmpStatusCode = fmpStatusCode,
            Suggestion = suggestion,
            Errors = errors
        };
    }

    public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}
