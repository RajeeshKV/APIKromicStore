namespace KromicStore.Application.Features.Authentication.DTOs;

/// <summary>
/// Returned by login, register, and token-refresh operations.
/// </summary>
public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int    ExpiresInSeconds,
    UserProfileResponse User
);
