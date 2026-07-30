using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenant;

public sealed record GetTenantQuery(Guid TenantId) : IRequest<GetTenantResponse>;

public sealed record GetTenantResponse(
    Guid TenantId,
    string Name,
    string StoreName,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<TenantDomainDto> Domains);

public sealed record TenantDomainDto(
    string? Subdomain,
    string? CustomDomain,
    bool IsPrimary,
    bool IsVerified);
