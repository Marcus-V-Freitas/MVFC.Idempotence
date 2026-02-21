namespace MVFC.Idempotence.Exceptions;

public sealed class IdempotencyException(string message, int statusCode = 500) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}