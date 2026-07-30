using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Initiates the password reset flow by generating a short-lived reset token.
/// Always returns success to prevent account enumeration.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest;
