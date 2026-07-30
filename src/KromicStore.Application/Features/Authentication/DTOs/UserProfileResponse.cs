namespace KromicStore.Application.Features.Authentication.DTOs;

/// <summary>
/// Embedded user profile returned inside AuthTokenResponse and GET /me.
/// </summary>
public sealed record UserProfileResponse(
    Guid    Id,
    Guid?   TenantId,
    string  Email,
    string  FirstName,
    string  LastName,
    bool    IsEmailVerified,
    IReadOnlyList<string> Roles
);
