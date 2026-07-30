using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Completes the password reset flow.
/// All active refresh tokens are revoked after a successful reset.
/// </summary>
public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest;
