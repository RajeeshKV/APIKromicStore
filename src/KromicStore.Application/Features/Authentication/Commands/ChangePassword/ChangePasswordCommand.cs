using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.ChangePassword;

/// <summary>
/// Allows an authenticated user to change their own password.
/// Requires the current password to confirm identity before accepting a new one.
/// All other sessions are revoked upon success.
/// </summary>
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
) : IRequest;
