namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Raised when an operation conflicts with existing state (e.g. duplicate email).
/// Maps to HTTP 409.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
