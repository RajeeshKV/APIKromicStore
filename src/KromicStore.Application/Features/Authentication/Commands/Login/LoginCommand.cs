using KromicStore.Application.Features.Authentication.DTOs;
using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Authenticates a user with email and password.
/// Returns JWT access token + rotating refresh token on success.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? DeviceName,
    string? IpAddress
) : IRequest<AuthTokenResponse>;
