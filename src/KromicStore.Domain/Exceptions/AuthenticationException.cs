namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Raised when credential validation fails.
/// Maps to HTTP 401.
/// </summary>
public sealed class AuthenticationException : DomainException
{
    public AuthenticationException(string message) : base(message) { }
}
