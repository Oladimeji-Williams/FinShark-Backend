namespace FinShark.Domain.Exceptions;

public sealed class ForbiddenOperationException(string message) : FinSharkException(message);
