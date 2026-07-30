namespace KromicStore.Application.Common.DTOs;

/// <summary>
/// Response DTO for authentication operations.
/// Contains JWT tokens and user information.
/// </summary>
public sealed class AuthenticationResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public int ExpiresInSeconds { get; set; }
}
