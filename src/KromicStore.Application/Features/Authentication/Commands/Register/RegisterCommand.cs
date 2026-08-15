using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using MediatR;

namespace KromicStore.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Registers a new tenant user account.
/// On success returns access + refresh tokens so the user is immediately logged in.
/// Email verification is sent but the account is usable for browsing;
/// privileged actions require IsEmailVerified = true.
/// </summary>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Subdomain,
    string? StoreName,
    string? DeviceName,
    string? IpAddress
) : IRequest<AuthTokenResponse>;
