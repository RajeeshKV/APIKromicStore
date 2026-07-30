using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Revokes the given refresh token.
/// The access token expires naturally (short-lived by design).
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest;
