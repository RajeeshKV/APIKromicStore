namespace KromicStore.Application.Common.Abstractions;

/// <summary>
/// Service for accessing currently authenticated user information from claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the unique identifier of the current authenticated user.
    /// Throws UnauthorizedAccessException if user is not authenticated.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets the email of the current user, if available.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets all roles assigned to the current user.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Gets a specific claim value by type.
    /// </summary>
    string? GetClaim(string claimType);
}
