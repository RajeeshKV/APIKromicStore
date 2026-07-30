namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Raised when a requested resource does not exist.
/// Maps to HTTP 404.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} '{key}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}
