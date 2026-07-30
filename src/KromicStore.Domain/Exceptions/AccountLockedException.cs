namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Raised when a user account is locked.
/// Maps to HTTP 423.
/// </summary>
public sealed class AccountLockedException : DomainException
{
    public AccountLockedException(string message) : base(message) { }
}
