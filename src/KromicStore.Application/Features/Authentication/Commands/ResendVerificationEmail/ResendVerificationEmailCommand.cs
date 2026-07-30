using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.ResendVerificationEmail;

/// <summary>
/// Invalidates any existing unused verification tokens for the user
/// and issues a fresh one. Rate-limiting is enforced at the API layer.
/// </summary>
public sealed record ResendVerificationEmailCommand(string Email) : IRequest;
