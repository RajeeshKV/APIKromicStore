namespace KromicStore.Application.Common.Abstractions;

/// <summary>
/// Abstraction over password hashing so the application layer never depends on
/// a specific hashing algorithm.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hash a plaintext password.</summary>
    string Hash(string password);

    /// <summary>Verify a plaintext password against a stored hash.</summary>
    bool Verify(string passwordHash, string providedPassword);
}
