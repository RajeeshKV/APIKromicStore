using KromicStore.Application.Features.Authentication.DTOs;
using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token: revokes the old one and issues a new pair.
/// Replay of a revoked token is treated as a potential compromise.
/// </summary>
public sealed record RefreshTokenCommand(
    string Token,
    string? DeviceName,
    string? IpAddress
) : IRequest<AuthTokenResponse>;
