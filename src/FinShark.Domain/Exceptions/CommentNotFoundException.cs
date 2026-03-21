namespace FinShark.Domain.Exceptions;

/// <summary>
/// Exception thrown when a comment is not found
/// </summary>
public sealed class CommentNotFoundException : FinSharkException
{
    public CommentNotFoundException(string message)
        : base(message)
    {
        ErrorCode = 1001;
    }
}
