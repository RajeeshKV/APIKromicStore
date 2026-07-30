namespace KromicStore.Domain.Exceptions;

/// <summary>
/// Raised when a user attempts a privileged action with an unverified email.
/// Maps to HTTP 403.
/// </summary>
public sealed class EmailNotVerifiedException : DomainException
{
    public EmailNotVerifiedException() : base("Email address has not been verified.") { }
}
