using MediatR;

namespace KromicStore.Application.Features.Authentication.Queries.CheckSubdomain;

/// <summary>
/// Checks whether a subdomain is available for registration.
/// Used by the UI to give real-time feedback as the user types.
/// </summary>
public sealed record CheckSubdomainQuery(string Subdomain) : IRequest<CheckSubdomainResult>;

public sealed record CheckSubdomainResult(
    bool   IsAvailable,
    string Subdomain,
    string? Reason = null);
