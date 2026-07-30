namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// Throw these to communicate business rule violations through the stack.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
