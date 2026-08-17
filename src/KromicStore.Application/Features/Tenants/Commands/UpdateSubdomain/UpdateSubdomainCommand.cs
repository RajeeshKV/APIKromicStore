using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateSubdomain;

/// <summary>
/// Changes the platform subdomain for a tenant (e.g. mystore → newname).
/// Validates the new subdomain is not reserved and not already taken.
/// Updates the TenantDomain row and the Tenant.Slug.
/// </summary>
public sealed record UpdateSubdomainCommand(
    Guid   TenantId,
    string NewSubdomain
) : IRequest<UpdateSubdomainResponse>;

public sealed record UpdateSubdomainResponse(
    Guid   TenantId,
    string Subdomain,
    string StoreUrl);
