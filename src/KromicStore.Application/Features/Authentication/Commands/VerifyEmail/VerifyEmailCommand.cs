using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Consumes an email verification token and marks the user's email as verified.
/// Token arrives via GET /api/v1/auth/verify-email?token=... link in email.
/// </summary>
public sealed record VerifyEmailCommand(string Token) : IRequest;
