using KromicStore.Application.Common.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace KromicStore.Infrastructure.Services;

/// <summary>
/// Password hashing backed by ASP.NET Core Identity's PasswordHasher.
/// Uses PBKDF2 with HMAC-SHA512, 100k iterations (Identity v3 format).
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // The generic type parameter is arbitrary for standalone use — it does not
    // tie this hasher to any Identity entity.
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));
        return _inner.HashPassword(new object(), password);
    }

    public bool Verify(string passwordHash, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(passwordHash, nameof(passwordHash));
        ArgumentNullException.ThrowIfNull(providedPassword, nameof(providedPassword));

        try
        {
            var result = _inner.VerifyHashedPassword(new object(), passwordHash, providedPassword);
            return result is PasswordVerificationResult.Success
                          or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            // Invalid Base64 hash format
            return false;
        }
    }
}
