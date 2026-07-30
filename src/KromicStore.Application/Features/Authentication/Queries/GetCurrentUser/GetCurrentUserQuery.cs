using KromicStore.Application.Features.Authentication.DTOs;
using MediatR;

namespace KromicStore.Application.Features.Authentication.Queries.GetCurrentUser;

/// <summary>
/// Returns the profile of the currently authenticated user.
/// Served by GET /api/v1/auth/me.
/// </summary>
public sealed record GetCurrentUserQuery : IRequest<UserProfileResponse>;
